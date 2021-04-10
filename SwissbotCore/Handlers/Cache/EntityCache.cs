using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwissbotCore
{
    
    public class DuplicateIdItemException : Exception
    {
        public DuplicateIdItemException() { }
        public DuplicateIdItemException(string message) : base(message) { }
        public DuplicateIdItemException(string message, Exception inner) : base(message, inner) { }
        protected DuplicateIdItemException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public class SingleGuildIDEntityCache<T> where T : class, IGuild
    {
        private Type _type = typeof(T);
        private List<T> _CacheList { get; set; } = new List<T>();
        public T this[ulong Id]
        {
            get => _CacheList.Any(x => x.Id == Id)
                ? _CacheList.Find(x => x.Id == Id)
                : null;
            set => AddOrReplace(value);
        }

        public List<T> ToList()
        {
            return _CacheList;
        }

        /// <summary>
        /// Returns the amount of items in the Cache
        /// </summary>
        public int Count
            => _CacheList.Count;

        /// <summary>
        /// Adds a new item in the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            // Since we should not have 2 users with the same id, check this :D
            if (_CacheList.Any(x => x.Id == value.Id))
                return;
            //throw new DuplicateIdItemException($"List already contains {value.GuildID} for type of {_type.Name}!");

            // We passed the check, so lets add it!
            _CacheList.Add(value);
        }

        /// <summary>
        /// Replaces an item in the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Replace(T value)
        {
            // Check if that item exists
            if (_CacheList.Any(x => x.Id == value.Id))
            {
                // Find the index of the requested item
                var indx = _CacheList.FindIndex(x => x.Id == value.Id);

                // Replace it with the value
                _CacheList[indx] = value;
            }
            else
            {
                // If it didn't exist in the list, add it to it
                Add(value);
            }
        }

        /// <summary>
        /// Adds an item into the cache, if it already exists then it will be replaced
        /// </summary>
        /// <param name="value"></param>
        public void AddOrReplace(T value)
        {
            if (_CacheList.Any(x => x.Id == value.Id))
                Replace(value);
            else
                Add(value);
        }

        /// <summary>
        /// Returns a result based on an expression
        /// </summary>
        /// <param name="predicate">The expression to search by</param>
        /// <returns></returns>
        public bool Any(Func<T, bool> predicate)
            => _CacheList.Any(predicate);

        /// <summary>
        /// Clears the Cache
        /// </summary>
        public void Clear()
            => _CacheList.Clear();

        /// <summary>
        /// Returns if the item exists in the Cache
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
            => _CacheList.Contains(value);

        /// <summary>
        /// Removes an item from the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Remove(T value)
            => _CacheList.Remove(value);
    }

    public class SingleIDEntityCache<T> where T : class, ISnowflakeEntity
    {
        private Type _type = typeof(T);
        private List<T> _CacheList { get; set; } = new List<T>();
        public T this[ulong Id] 
        { 
            get => _CacheList.Any(x => x.Id == Id) 
                ? _CacheList.Find(x => x.Id == Id) 
                : null; 
            set => AddOrReplace(value); 
        }

        public List<T> ToList()
        {
            return _CacheList;
        }

        /// <summary>
        /// Returns the amount of items in the Cache
        /// </summary>
        public int Count 
            => _CacheList.Count;

        /// <summary>
        /// Adds a new item in the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            // Since we should not have 2 users with the same id, check this :D
            if (_CacheList.Any(x => x.Id == value.Id))
                return;
            //throw new DuplicateIdItemException($"List already contains {value.Id} for type of {_type.Name}!");

            // We passed the check, so lets add it!
            _CacheList.Add(value);
        }

        /// <summary>
        /// Replaces an item in the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Replace(T value)
        {
            // Check if that item exists
            if (_CacheList.Any(x => x.Id == value.Id))
            {
                // Find the index of the requested item
                var indx = _CacheList.FindIndex(x => x.Id == value.Id);

                // Replace it with the value
                _CacheList[indx] = value;
            }
            else
            {
                // If it didn't exist in the list, add it to it
                Add(value);
            }
        }

        /// <summary>
        /// Adds an item into the cache, if it already exists then it will be replaced
        /// </summary>
        /// <param name="value"></param>
        public void AddOrReplace(T value)
        {
            if (_CacheList.Any(x => x.Id == value.Id))
                Replace(value);
            else
                Add(value);
        }
        
        /// <summary>
        /// Returns a result based on an expression
        /// </summary>
        /// <param name="predicate">The expression to search by</param>
        /// <returns></returns>
        public bool Any(Func<T, bool> predicate)
            => _CacheList.Any(predicate);

        /// <summary>
        /// Clears the Cache
        /// </summary>
        public void Clear()
            => _CacheList.Clear();

        /// <summary>
        /// Returns if the item exists in the Cache
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
            => _CacheList.Contains(value);

        /// <summary>
        /// Removes an item from the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Remove(T value)
            => _CacheList.Remove(value);
    }
    public class DoubleIDEntityCache<T> where T : class, IGuildUser
    {
        private Type _type = typeof(T);

        private List<T> _CacheList { get; set; } = new List<T>();
        public T this[ulong Id, ulong GuildID] 
        { 
            get => _CacheList.Any(x => x.Id == Id && x.GuildId == GuildID) 
                ? _CacheList.Find(x => x.Id == Id && x.GuildId == GuildID) 
                : null; 
            set => AddOrReplace(value); 
        }

        /// <summary>
        ///     Pops a user from the front of the list
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if(_CacheList.Count > 0)
            {
                T u = _CacheList[0];
                _CacheList.Remove(u);
                return u;
            }
            return null;
        }

        /// <summary>
        /// Returns the amount of items in the Cache
        /// </summary>
        public int Count
            => _CacheList.Count;

        /// <summary>
        /// Adds a new item in the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            if (value == null)
            {
                var stack = Environment.StackTrace;
                Console.WriteLine($"Failed on add: {stack}");
                return;
            }

            // Since we should not have 2 users with the same id, check this :D
            if (_CacheList.Any(x => x.Id == value.Id && x.GuildId == value.GuildId))
                return;
                //throw new DuplicateIdItemException($"List already contains {value.Id} for type of {_type.Name}!");

            // We passed the check, so lets add it!
            _CacheList.Add(value);

            // Invoke the add event
        }

        /// <summary>
        /// Replaces an item in the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Replace(T value)
        {
            if (value == null)
            {
                var stack = Environment.StackTrace;
                Console.WriteLine($"Failed on add: {stack}");
                return;
            }
            // Check if that item exists
            if (_CacheList.Any(x => x.Id == value.Id && x.GuildId == value.GuildId))
            {
                // Find the index of the requested item
                var indx = _CacheList.FindIndex(x => x.Id == value.Id && x.GuildId == value.GuildId);

                // Replace it with the value
                _CacheList[indx] = value;
            }
            else
            {
                // If it didn't exist in the list, add it to it
                Add(value);
            }
        }

        /// <summary>
        /// Adds an item into the cache, if it already exists then it will be replaced
        /// </summary>
        /// <param name="value"></param>
        public void AddOrReplace(T value)
        {
            if (value == null)
            {
                var stack = Environment.StackTrace;
                Console.WriteLine($"Failed on add: {stack}");
                return;
            }

            if (_CacheList.Any(x => x.Id == value.Id && x.GuildId == value.GuildId))
                Replace(value);
            else
                Add(value);
        }

        /// <summary>
        /// Returns a result based on an expression
        /// </summary>
        /// <param name="predicate">The expression to search by</param>
        /// <returns></returns>
        public bool Any(Func<T, bool> predicate)
            => _CacheList.Any(predicate);

        /// <summary>
        /// Clears the Cache
        /// </summary>
        public void Clear()
            => _CacheList.Clear();

        /// <summary>
        /// Returns if the item exists in the Cache
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
            => _CacheList.Contains(value);

        /// <summary>
        /// Removes an item from the Cache
        /// </summary>
        /// <param name="value"></param>
        public void Remove(T value)
            => _CacheList.Remove(value);
    }
}
