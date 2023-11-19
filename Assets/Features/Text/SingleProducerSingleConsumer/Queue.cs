using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MagicSwords.Features.Text.SingleProducerSingleConsumer
{
    /// <summary>
    /// A common interface that represents the producer side of a concurrent collection.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the collection.</typeparam>
    internal interface IProducerQueue<in T>
    {
        /// <summary>
        /// Attempts to add the object at the end of the <see cref="IProducerQueue{T}"/>.
        /// </summary>
        bool TryEnqueue(T value);

        UniTask EnqueueAsync(T value, PlayerLoopTiming yieldPoint = PlayerLoopTiming.Update, CancellationToken cancellation = default);
    }

    /// <summary>
    /// A common interface that represents the consumer side of a concurrent queue.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    internal interface IConsumerQueue<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets a value that indicates whether the <see cref="IConsumerQueue{T}"/> is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Attempts to add the object at the end of the <see cref="IConsumerQueue{T}"/>.
        /// </summary>
        bool TryDequeue(out T? value);

        /// <summary>
        /// Attempts to return an object from the beginning of the <see cref="IConsumerQueue{T}"/>
        /// without removing it.
        /// </summary>
        bool TryPeek(out T? value);

        /// <summary>
        /// Copies the elements stored in the <see cref="IConsumerQueue{T}"/> to a new array.
        /// </summary>
        T[] ToArray();

        /// <summary>
        /// Copies the <see cref="IConsumerQueue{T}"/> elements to an existing one-dimensional <see
        /// cref="Array">Array</see>, starting at the specified array index.
        /// </summary>
        void CopyTo(T[] array, int index);
    }

    // ReSharper disable once InheritdocConsiderUsage
    /// <summary>
    /// Represents a thread-safe first-in, first-out collection of objects.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    /// <remarks>
    /// Can be used with one producer thread and one consumer thread.
    /// </remarks>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    internal sealed class Queue<T> : IProducerConsumerCollection<T>, IReadOnlyCollection<T>, IProducerQueue<T>, IConsumerQueue<T>
    {
        private readonly T[] _items;
        private HeadAndTail _headAndTail;

        /// <summary>
        /// Gets the capacity of this <see cref="Queue{T}"/>.
        /// </summary>
        public int Capacity => _items.Length - 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Queue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The fixed-capacity of this <see cref="Queue{T}"/></param>
        public Queue(uint capacity)
        {
            // Reserve one empty slot
            capacity++;
            _items = new T[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Queue{T}"/> class that contains elements copied
        /// from the specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements are copied to the new <see cref="Queue{T}"/>.
        /// </param>
        public Queue(ICollection<T> collection)
        {
            // Reserve one empty slot
            var capacity = collection.Count + 1;

            _items = new T[capacity];
            collection.CopyTo(_items, 0);

            // Increment tail
            _headAndTail.Tail += collection.Count;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets a value that indicates whether the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> is empty.
        /// Value becomes stale after more enqueue or dequeue operations.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                var nextHead = Volatile.Read(ref _headAndTail.Head) + 1;

                return Volatile.Read(ref _headAndTail.Tail) < nextHead;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Queue{T}"/>.
        /// Value becomes stale after more enqueue or dequeue operations.
        /// </summary>
        public int Count
        {
            get
            {
                var head = Volatile.Read(ref _headAndTail.Head);
                var tail = Volatile.Read(ref _headAndTail.Tail);

                var dif = tail - head;
                if (dif < 0)
                {
                    dif += _items.Length;
                }

                return dif;
            }
        }

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => throw new NotSupportedException("The SyncRoot property may not be used for the synchronization of concurrent collections.");
        bool IProducerConsumerCollection<T>.TryAdd(T item) => ((IProducerQueue<T>)this).TryEnqueue(item);
        bool IProducerConsumerCollection<T>.TryTake(out T item) => ((IConsumerQueue<T>)this).TryDequeue(out item!);

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is T[] szArray)
            {
                CopyTo(szArray, index);

                return;
            }

            ToArray().CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Queue{T}"/>.
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        /// <inheritdoc />
        /// <summary>
        /// Attempts to add the object at the end of the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" />.
        /// Returns false if the queue is full.
        /// </summary>
        bool IProducerQueue<T>.TryEnqueue(T item)
        {
            var tail = Volatile.Read(ref _headAndTail.Tail);
            var nextTail = GetNext(tail, _items.Length);

            // Full Queue
            if (nextTail == Volatile.Read(ref _headAndTail.Head)) return false;

            _items[tail] = item;

            Volatile.Write(ref _headAndTail.Tail, nextTail);
            return true;
        }

        UniTask IProducerQueue<T>.EnqueueAsync(T value, PlayerLoopTiming yieldPoint, CancellationToken cancellation)
        {
            var writer = (IProducerQueue<T>) this;

            return UniTask.WaitUntil
            (
                predicate: () => writer.TryEnqueue(value),
                yieldPoint,
                cancellation,
                cancelImmediately: true

            ).SuppressCancellationThrow()
                .AsUniTask();
        }

        /// <inheritdoc />
        /// <summary>
        /// Attempts to remove and return the object at the beginning of the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" />.
        /// Returns false if the queue is empty.
        /// </summary>
        bool IConsumerQueue<T>.TryDequeue(out T? item)
        {
            var head = Volatile.Read(ref _headAndTail.Head);

            // Queue empty
            if (Volatile.Read(ref _headAndTail.Tail) == head)
            {
                item = default;

                return false;
            }

            item = _items[head];

            var nextHead = GetNext(head, _items.Length);
            Volatile.Write(ref _headAndTail.Head, nextHead);

            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Attempts to return an object from the beginning of the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> without removing it.
        /// Returns false if the queue if empty.
        /// </summary>
        bool IConsumerQueue<T>.TryPeek(out T? item)
        {
            var head = Volatile.Read(ref _headAndTail.Head);

            // Queue empty
            if (Volatile.Read(ref _headAndTail.Tail) == head)
            {
                item = default;

                return false;
            }

            item = _items[head];

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNext(int value, int length)
        {
            value++;

            if (value == length)
            {
                value = 0;
            }

            return value;
        }

        /// <inheritdoc cref="IConsumerQueue{T}.ToArray" />
        /// <summary>
        /// Copies the elements stored in the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> to a new array.
        /// Consumer-Thread-safe
        /// </summary>
        private T[] ToArray()
        {
            var head = Volatile.Read(ref _headAndTail.Head);
            var tail = Volatile.Read(ref _headAndTail.Tail);

            var count = tail - head;
            if (count < 0)
            {
                count += _items.Length;
            }

            if (count <= 0) return Array.Empty<T>();

            var arr = new T[count];

            var numToCopy = count;
            var bufferLength = _items.Length;
            var firstPart = Math.Min(bufferLength - head, numToCopy);

            Array.Copy(_items, head, arr, 0, firstPart);
            numToCopy -= firstPart;

            if (numToCopy > 0)
            {
                Array.Copy(_items, 0, arr, 0 + bufferLength - head, numToCopy);
            }

            return arr;
        }

        /// <inheritdoc cref="IConsumerQueue{T}.ToArray" />
        /// <summary>
        /// Copies the elements stored in the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> to a new array.
        /// Consumer-Thread-safe
        /// </summary>
        T[] IConsumerQueue<T>.ToArray() => ToArray();

        /// <inheritdoc cref="IConsumerQueue{T}.ToArray" />
        /// <summary>
        /// Copies the elements stored in the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> to a new array.
        /// Consumer-Thread-safe
        /// </summary>
        T[] IProducerConsumerCollection<T>.ToArray() => ToArray();

        /// <inheritdoc cref="IConsumerQueue{T}.CopyTo" />
        /// <summary>
        /// Copies the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> elements to an existing <see cref="T:System.Array">Array</see>, starting at the specified array index.
        /// Consumer-Thread-safe
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the elements copied from the
        /// <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" />. The <see cref="T:System.Array">Array</see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        private void CopyTo(T[] array, int index)
        {
            if ((uint)index > array.Length) throw new ArgumentOutOfRangeException(nameof(index), index, "Index was out of range. Must be non - negative and less than the size of the collection.");

            var head = Volatile.Read(ref _headAndTail.Head);
            var tail = Volatile.Read(ref _headAndTail.Tail);

            var count = tail - head;
            if (count < 0)
            {
                count += _items.Length;
            }

            if (index > array.Length + count) throw new ArgumentException("Destination array is not long enough to copy all the items in the collection.Check array index and length.");

            if (count <= 0) return;

            var numToCopy = count;
            var bufferLength = _items.Length;
            var firstPart = Math.Min(bufferLength - head, numToCopy);

            Array.Copy(_items, head, array, index, firstPart);
            numToCopy -= firstPart;

            if (numToCopy > 0)
            {
                Array.Copy(_items, 0, array, index + bufferLength - head, numToCopy);
            }
        }

        /// <inheritdoc cref="IConsumerQueue{T}.CopyTo" />
        /// <summary>
        /// Copies the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> elements to an existing <see cref="T:System.Array">Array</see>, starting at the specified array index.
        /// Consumer-Thread-safe
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the elements copied from the
        /// <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" />. The <see cref="T:System.Array">Array</see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        void IConsumerQueue<T>.CopyTo(T[] array, int index)
        {
            CopyTo(array, index);
        }

        /// <inheritdoc cref="IConsumerQueue{T}.CopyTo" />
        /// <summary>
        /// Copies the <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" /> elements to an existing <see cref="T:System.Array">Array</see>, starting at the specified array index.
        /// Consumer-Thread-safe
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the elements copied from the
        /// <see cref="T:MagicSwords.Features.Text.SingleProducerSingleConsumer.Queue`1" />. The <see cref="T:System.Array">Array</see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        void IProducerConsumerCollection<T>.CopyTo(T[] array, int index)
        {
            CopyTo(array, index);
        }

        /// <summary>
        /// Removes all objects from the <see cref="Queue{T}"/>.
        /// This method is NOT thread-safe!
        /// </summary>
        public void Clear()
        {
            var head = Volatile.Read(ref _headAndTail.Head);
            var tail = Volatile.Read(ref _headAndTail.Tail);

            var count = tail - head;
            if (count < 0)
                count += _items.Length;

            var numToCopy = count;
            var bufferLength = _items.Length;
            var firstPart = Math.Min(bufferLength - head, numToCopy);

            // Clear first part.
            Array.Clear(_items, head, firstPart);
            numToCopy -= firstPart;

            // Clear second part.
            if (numToCopy > 0)
            {
                Array.Clear(_items, 0, numToCopy);
            }

            _headAndTail = new HeadAndTail();
        }

        /// <summary>
        /// Defines an enumerator for <see cref="Queue{T}"/>
        /// </summary>
        private struct Enumerator : IEnumerator<T>
        {
            // Enumerates over the provided Single Producer Single Consumer RingBuffer. Enumeration counts as a READ/Consume operation.
            // The amount of items enumerated can vary depending on if the TAIL moves during enumeration.
            // The HEAD is frozen in place when the enumerator is created. This means that the maximum
            // amount of items read is always the capacity of the queue and no more.

            private readonly Queue<T> _queue;
            private readonly int _headStart;
            private readonly int _capacity;

            private int _index;
            private T _current;

            internal Enumerator(Queue<T> queue)
            {
                _queue = queue;
                _index = -1;
                _current = default!;
                _capacity = queue._items.Length;
                _headStart = Volatile.Read(ref queue._headAndTail.Head);
            }

            /// <summary>
            /// Disposes the enumerator.
            /// </summary>
            void IDisposable.Dispose() => _index = -2;

            /// <summary>
            /// Moves the enumerator to the next position.
            /// </summary>
            bool IEnumerator.MoveNext()
            {
                if (_index == -2) return false;

                var head = Volatile.Read(ref _queue._headAndTail.Head);
                if (_headStart != head) throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                var headIndex = head + ++_index;
                if (headIndex >= _capacity)
                {
                    // Wrap around if needed
                    headIndex -= _capacity;
                }

                // Queue empty
                if (Volatile.Read(ref _queue._headAndTail.Tail) == headIndex)
                {
                    return false;
                }

                _current = _queue._items[headIndex];

                return true;
            }

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            void IEnumerator.Reset() => _index = -1;

            /// <summary>
            /// Gets the current object.
            /// </summary>
            readonly T IEnumerator<T>.Current => _current;
            readonly object IEnumerator.Current => _current!;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 3 * CacheLineSize)]
    [DebuggerDisplay("Head = {Head}, Tail = {Tail}")]
    internal struct HeadAndTail
    {
        private const int CacheLineSize =
#       if TARGET_ARM64
            128;
#       else
            64;
#       endif

        [FieldOffset(1 * CacheLineSize)]
        public int Head;

        [FieldOffset(2 * CacheLineSize)]
        public int Tail;
    }
}
