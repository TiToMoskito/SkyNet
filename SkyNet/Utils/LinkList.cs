//using System;

//namespace SkyNet.Utils
//{
//    public class LinkList<T> where T : class, LinkList<T>.IItem
//    {

//        public interface IItem
//        {
//            T Next { get; set; }
//            T Prev { get; set; }
//            LinkList<T> List { get; set; }
//        }

//        public struct Iterator
//        {
//            T _head;
//            Int32 _count;

//            readonly Int32 _version;
//            readonly LinkList<T> _list;

//            public Iterator(LinkList<T> list)
//            {
//                Assert.Check(list != null);

//                _list = list;
//                _head = list._head;
//                _count = list._count;
//                _version = list._version;
//            }

//            public Boolean Next(out T item)
//            {
//                if (_version != _list._version)
//                {
//                    throw new InvalidOperationException("List has been modified");
//                }

//                if (_count > 0)
//                {
//                    _count -= 1;

//                    // assign head
//                    item = _head;

//                    // move head forward
//                    _head = item.Next;

//                    return true;
//                }

//                item = default(T);
//                return false;
//            }

//        }

//        // total count
//        Int32 _count;

//        // list version
//        Int32 _version;

//        // head reference
//        T _head;

//        public Int32 Count
//        {
//            get
//            {
//                return _count;
//            }
//        }

//        public T Head
//        {
//            get
//            {
//                Assert.Check(_count > 0);
//                Assert.Check(_head != null);
//                return Head;
//            }
//        }

//        public T Tail
//        {
//            get
//            {
//                Assert.Check(_count > 0);
//                Assert.Check(_head != null);
//                return Head.Prev;
//            }
//        }

//        public Iterator GetIterator()
//        {
//            return new Iterator(this);
//        }

//        public T PopFirst()
//        {
//            var item = _head;
//            Remove(_head);
//            return item;
//        }

//        public T PopLast()
//        {
//            var item = _head.Prev;
//            Remove(_head.Prev);
//            return item;
//        }

//        public void AddFirst(T item)
//        {
//            if (ReferenceEquals(_head, null))
//            {
//                AddEmpty(item);
//            }
//            else
//            {
//                AddBefore(item, _head);

//                // gotta replace head here since we're replacing the first element
//                _head = item;
//            }
//        }

//        public void AddLast(T item)
//        {
//            if (ReferenceEquals(_head, null))
//            {
//                AddEmpty(item);
//            }
//            else
//            {
//                AddBefore(item, _head);
//            }
//        }

//        public void Remove(T item)
//        {
//            Assert.Check(ReferenceEquals(item.List, this));

//            if (_count == 1)
//            {
//                Assert.Check(ReferenceEquals(item, _head));

//                _head = null;
//            }
//            else
//            {
//                // splice out item by taking prev/next 
//                // from it and hooking them together
//                var prev = item.Prev;
//                var next = item.Next;

//                prev.Next = item.Next;
//                next.Prev = item.Prev;

//                // if this was the head, we need to make Next the new head
//                if (ReferenceEquals(_head, item))
//                {
//                    _head = item.Next;
//                }
//            }

//            // clear next/prev/list from item we removed
//            item.Next = null;
//            item.Prev = null;
//            item.List = null;

//            _count -= 1;
//            _version += 1;
//        }

//        public Boolean InList(T item)
//        {
//            if (ReferenceEquals(item.List, this))
//            {
//                Assert.Check(ReferenceEquals(item.Next, null) == false);
//                Assert.Check(ReferenceEquals(item.Prev, null) == false);
//                return true;
//            }
//            else
//            {
//                Assert.Check(ReferenceEquals(item.Next, null));
//                Assert.Check(ReferenceEquals(item.Prev, null));
//                return false;
//            }
//        }

//        public static Boolean InAnyList(T item)
//        {
//            if (ReferenceEquals(item.List, null))
//            {
//                Assert.Check(ReferenceEquals(item.Next, null));
//                Assert.Check(ReferenceEquals(item.Prev, null));
//                return false;
//            }
//            else
//            {
//                Assert.Check(ReferenceEquals(item.Next, null) == false);
//                Assert.Check(ReferenceEquals(item.Prev, null) == false);
//                return true;
//            }
//        }

//        void AddEmpty(T item)
//        {
//            Assert.Check(_count == 0);
//            Assert.Check(ReferenceEquals(_head, null));
//            Assert.Check(ReferenceEquals(item.List, null));

//            item.Next = item;
//            item.Prev = item;
//            item.List = this;

//            _head = item;
//            _count = 1;
//            _version += 1;
//        }

//        void AddBefore(T item, T before)
//        {
//            Assert.Check(_count > 0);
//            Assert.Check(ReferenceEquals(_head, null) == false);
//            Assert.Check(ReferenceEquals(item.List, null));

//            // Next for item we are inserting is the before node
//            item.Next = before;

//            // previous for the item we're inserting is the old Prev of the before node
//            item.Prev = before.Prev;

//            // Next for the Prev node of the before is the inserted item itself
//            before.Prev.Next = item;

//            // last step, the Prev node of the before is the item we're inserting
//            before.Prev = item;

//            // set list
//            item.List = this;

//            _count += 1;
//            _version += 1;
//        }
//    }
//}
