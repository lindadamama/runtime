// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#ifndef _LIR_H_
#define _LIR_H_

class Compiler;
struct GenTree;
struct BasicBlock;
class Rationalizer;

class LIR final
{
public:
    class Range;

    //------------------------------------------------------------------------
    // LIR::Flags: Defines the set of flags that may appear in the
    //             GenTree::gtLIRFlags field.
    class Flags final
    {
        // Disallow the creation of values of this type.
        Flags() = delete;

    public:
        enum : unsigned char
        {
            None = 0x00,

            Mark = 0x01, // An arbitrary "mark" bit that can be used in place of
                         // a more expensive data structure when processing a set
                         // of LIR nodes. See for example `LIR::GetTreeRange`.

            UnusedValue = 0x02, // Set on a node if it produces a value that is not
                                // subsequently used. Should never be set on nodes
                                // that return `false` for `GenTree::IsValue`. Note
                                // that this bit should not be assumed to be valid
                                // at all points during compilation: it is currently
                                // only computed during target-dependent lowering.

            RegOptional = 0x04, // Set on a node if it produces a value, but does not
                                // require a register (i.e. it can be used from memory).
        };
    };

    //------------------------------------------------------------------------
    // LIR::Use: Represents a use <-> def edge between two nodes in a range
    //           of LIR. Provides utilities to point the use to a different
    //           def. Note that because this type deals in edges between
    //           nodes, it represents the single use of the def.
    //
    class Use final
    {
    private:
        Range*    m_range;
        GenTree** m_edge;
        GenTree*  m_user;

    public:
        Use();
        Use(const Use& other);
        Use(Range& range, GenTree** edge, GenTree* user);

        Use& operator=(const Use& other);
        Use& operator=(Use&& other);

        static void MakeDummyUse(Range& range, GenTree* node, Use* dummyUse);

        GenTree* Def() const;
        GenTree* User() const;

        bool IsInitialized() const;
        void AssertIsValid() const;
        bool IsDummyUse() const;

        void     ReplaceWith(GenTree* replacement);
        unsigned ReplaceWithLclVar(Compiler* compiler, unsigned lclNum = BAD_VAR_NUM, GenTree** pStore = nullptr);
    };

    //------------------------------------------------------------------------
    // LIR::ReadOnlyRange:
    //
    // Represents a contiguous range of LIR nodes that may be a subrange of
    // a containing range. Provides a small set of utilities for iteration.
    // Instances of this type are primarily created by and provided to
    // analysis and utility methods on LIR::Range.
    //
    // Although some pains have been taken to help guard against the existence
    // of invalid subranges, it remains possible to create them. For example,
    // consider the following:
    //
    //     // View the block as a range
    //     LIR::Range& blockRange = LIR::AsRange(block);
    //
    //     // Create a read only range from from it.
    //     LIR::ReadOnlyRange readRange = blockRange;
    //
    //     // Remove the last node from the block
    //     blockRange.Remove(blockRange.LastNode());
    //
    // After the removal of the last node in the block, the last node of
    // readRange is no longer linked to any of the other nodes in readRange. Due
    // to issues such as the above, some care must be taken in order to
    // ensure that ranges are not used once they have been invalidated.
    //
    class ReadOnlyRange
    {
        friend class LIR;
        friend class Range;
        friend struct BasicBlock;

    private:
        GenTree* m_firstNode;
        GenTree* m_lastNode;

        ReadOnlyRange(const ReadOnlyRange& other)            = delete;
        ReadOnlyRange& operator=(const ReadOnlyRange& other) = delete;

    public:
        ReadOnlyRange(GenTree* firstNode, GenTree* lastNode);

        class Iterator
        {
            friend class ReadOnlyRange;

            GenTree* m_node;

            Iterator(GenTree* begin)
                : m_node(begin)
            {
            }

        public:
            Iterator()
                : m_node(nullptr)
            {
            }

            inline GenTree* operator*()
            {
                return m_node;
            }

            inline GenTree* operator->()
            {
                return m_node;
            }

            inline bool operator==(const Iterator& other) const
            {
                return m_node == other.m_node;
            }

            inline bool operator!=(const Iterator& other) const
            {
                return m_node != other.m_node;
            }

            inline Iterator& operator++()
            {
                m_node = (m_node == nullptr) ? nullptr : m_node->gtNext;
                return *this;
            }
        };

        class ReverseIterator
        {
            friend class ReadOnlyRange;

            GenTree* m_node;

            ReverseIterator(GenTree* begin)
                : m_node(begin)
            {
            }

        public:
            ReverseIterator()
                : m_node(nullptr)
            {
            }

            inline GenTree* operator*()
            {
                return m_node;
            }

            inline GenTree* operator->()
            {
                return m_node;
            }

            inline bool operator==(const ReverseIterator& other) const
            {
                return m_node == other.m_node;
            }

            inline bool operator!=(const ReverseIterator& other) const
            {
                return m_node != other.m_node;
            }

            inline ReverseIterator& operator++()
            {
                m_node = (m_node == nullptr) ? nullptr : m_node->gtPrev;
                return *this;
            }
        };

        ReadOnlyRange();
        ReadOnlyRange(ReadOnlyRange&& other);

        GenTree* FirstNode() const;
        GenTree* LastNode() const;

        bool IsEmpty() const;

        Iterator begin() const;
        Iterator end() const;

        ReverseIterator rbegin() const;
        ReverseIterator rend() const;

#ifdef DEBUG
        bool Contains(GenTree* node) const;
#endif

        ReadOnlyRange& operator=(ReadOnlyRange&& other);
    };

    //------------------------------------------------------------------------
    // LIR::Range:
    //
    // Represents a contiguous range of LIR nodes. Provides a variety of
    // variety of utilities that modify the LIR contained in the range. Unlike
    // `ReadOnlyRange`, values of this type may be edited.
    //
    // Because it is not a final class, it is possible to slice values of this
    // type; this is especially dangerous when the Range value is actually of
    // type `BasicBlock`. As a result, this type is not copyable and it is
    // not possible to view a `BasicBlock` as anything other than a `Range&`.
    //
    class Range : public ReadOnlyRange
    {
        friend class LIR;
        friend struct BasicBlock;
        friend class Rationalizer;

    private:
        Range(GenTree* firstNode, GenTree* lastNode);

        Range(const Range& other)            = delete;
        Range& operator=(const Range& other) = delete;

        template <bool markFlagsOperands = false>
        ReadOnlyRange GetMarkedRange(unsigned markCount, GenTree* start, bool* isClosed, unsigned* sideEffects) const;

        void FinishInsertBefore(GenTree* insertionPoint, GenTree* first, GenTree* last);
        void FinishInsertAfter(GenTree* insertionPoint, GenTree* first, GenTree* last);

    public:
        Range();
        Range(Range&& other);

        GenTree* FirstNonCatchArgNode() const;

        void InsertBefore(GenTree* insertionPoint, GenTree* node);
        void InsertAfter(GenTree* insertionPoint, GenTree* node);

        void InsertBefore(GenTree* insertionPoint, GenTree* node1, GenTree* node2);
        void InsertBefore(GenTree* insertionPoint, GenTree* node1, GenTree* node2, GenTree* node3);
        void InsertBefore(GenTree* insertionPoint, GenTree* node1, GenTree* node2, GenTree* node3, GenTree* node4);

        void InsertAfter(GenTree* insertionPoint, GenTree* node1, GenTree* node2);
        void InsertAfter(GenTree* insertionPoint, GenTree* node1, GenTree* node2, GenTree* node3);
        void InsertAfter(GenTree* insertionPoint, GenTree* node1, GenTree* node2, GenTree* node3, GenTree* node4);

        void InsertBefore(GenTree* insertionPoint, Range&& range);
        void InsertAfter(GenTree* insertionPoint, Range&& range);

        void InsertAtBeginning(GenTree* node);
        void InsertAtEnd(GenTree* node);

        void InsertAtBeginning(Range&& range);
        void InsertAtEnd(Range&& range);

        template <typename... Trees>
        void InsertAtBeginning(GenTree* tree, Trees&&... rest)
        {
            InsertAtBeginning(std::forward<Trees>(rest)...);
            InsertAtBeginning(tree);
        }

        template <typename... Trees>
        void InsertAtEnd(GenTree* tree, Trees&&... rest)
        {
            InsertAtEnd(tree);
            InsertAtEnd(std::forward<Trees>(rest)...);
        }

        void  Remove(GenTree* node, bool markOperandsUnused = false);
        Range Remove(GenTree* firstNode, GenTree* lastNode);
        Range Remove(ReadOnlyRange&& range);

        void Delete(Compiler* compiler, BasicBlock* block, GenTree* node);
        void Delete(Compiler* compiler, BasicBlock* block, GenTree* firstNode, GenTree* lastNode);
        void Delete(Compiler* compiler, BasicBlock* block, ReadOnlyRange&& range);

        bool TryGetUse(GenTree* node, Use* use);

        ReadOnlyRange GetTreeRange(GenTree* root, bool* isClosed) const;
        ReadOnlyRange GetTreeRange(GenTree* root, bool* isClosed, unsigned* sideEffects) const;
#ifdef DEBUG
        ReadOnlyRange GetTreeRangeWithFlags(GenTree* root, bool* isClosed, unsigned* sideEffects) const;
#endif
        ReadOnlyRange GetRangeOfOperandTrees(GenTree* root, bool* isClosed, unsigned* sideEffects) const;

#ifdef DEBUG
        bool CheckLIR(Compiler* compiler, bool checkUnusedValues = false) const;
#endif
    };

public:
    static Range&       AsRange(BasicBlock* block);
    static const Range& AsRange(const BasicBlock* block);

    static Range EmptyRange();
    static Range SeqTree(Compiler* compiler, GenTree* tree);

    static void InsertBeforeTerminator(BasicBlock* block, LIR::Range&& range);

    static GenTree* LastNode(GenTree* node1, GenTree* node2);
    static GenTree* LastNode(GenTree** nodes, size_t numNodes);
    static GenTree* FirstNode(GenTree* node1, GenTree* node2);
};

inline void GenTree::SetUnusedValue()
{
    gtLIRFlags |= LIR::Flags::UnusedValue;
    ClearContained();
}

inline void GenTree::ClearUnusedValue()
{
    gtLIRFlags &= ~LIR::Flags::UnusedValue;
}

inline bool GenTree::IsUnusedValue() const
{
    return (gtLIRFlags & LIR::Flags::UnusedValue) != 0;
}

inline void GenTree::SetRegOptional()
{
    gtLIRFlags |= LIR::Flags::RegOptional;
}

inline void GenTree::ClearRegOptional()
{
    gtLIRFlags &= ~LIR::Flags::RegOptional;
}

inline bool GenTree::IsRegOptional() const
{
    return (gtLIRFlags & LIR::Flags::RegOptional) != 0;
}

template <typename T, T* T::*prev, T* T::*next>
static void CheckDoublyLinkedList(T* first)
{
    // (1) ensure there are no circularities, (2) ensure the prev list is
    // precisely the inverse of the gtNext list.
    //
    // To detect circularity, use the "tortoise and hare" 2-pointer algorithm.

    if (first == nullptr)
    {
        return;
    }

    GenTree* slowNode = first;
    assert(slowNode != nullptr);
    GenTree* fastNode1    = nullptr;
    GenTree* fastNode2    = slowNode;
    GenTree* prevSlowNode = nullptr;
    while (((fastNode1 = fastNode2->*next) != nullptr) && ((fastNode2 = fastNode1->*next) != nullptr))
    {
        if ((slowNode == fastNode1) || (slowNode == fastNode2))
        {
            assert(!"Circularity detected");
        }
        assert(slowNode->*prev == prevSlowNode && "Invalid prev link");
        prevSlowNode = slowNode;
        slowNode     = slowNode->*next;
        assert(slowNode != nullptr); // the fastNodes would have gone null first.
    }
    // If we get here, the list had no circularities, so either fastNode1 or fastNode2 must be nullptr.
    assert((fastNode1 == nullptr) || (fastNode2 == nullptr));

    // Need to check the rest of the gtPrev links.
    while (slowNode != nullptr)
    {
        assert(slowNode->*prev == prevSlowNode && "Invalid prev link");
        prevSlowNode = slowNode;
        slowNode     = slowNode->*next;
    }
}

#endif // _LIR_H_
