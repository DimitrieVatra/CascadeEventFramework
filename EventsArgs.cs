using System;
using System.Collections.Generic;
using System.Reflection;

namespace CascadeEventFramework
{
    public class BeforeUpdatedEventArgs : ItemEventArgs
    {
        public PropertyInfo PropertyInfo { get; private set; }

        public BeforeUpdatedEventArgs(Item item, PropertyInfo propertyInfo) : base(item)
        {
            PropertyInfo = propertyInfo;
        }
        public new BeforeUpdatedEventArgs Stack(object item)
        {
            return base.Stack(item) as BeforeUpdatedEventArgs;
        }
    }
    public interface IBeforeUpdatedEventArgs<out ItemType> where ItemType : Item { }
    public class BeforeUpdatedEventArgs<ItemType> : BeforeUpdatedEventArgs, IItemEventArgs<ItemType> where ItemType : Item
    {
        public new ItemType Item { get => base.Item as ItemType; private set => base.Item = value; }
        public BeforeUpdatedEventArgs(Item item, PropertyInfo propertyName) : base(item, propertyName) { }
        public BeforeUpdatedEventArgs(BeforeUpdatedEventArgs args) : base(args.Item, args.PropertyInfo) { }
    }
    public class ItemWithPropertyEventArgs<ItemType> : ItemWithPropertyEventArgs, IItemEventArgs<ItemType> where ItemType : Item
    {
        public new ItemType Item { get => base.Item as ItemType; private set => base.Item = value; }
        public ItemWithPropertyEventArgs(ItemWithPropertyEventArgs args) : base(args.Item, args.Property)
        {
            InvokeHistory = args.InvokeHistory;
        }
        public new ItemWithPropertyEventArgs<ItemType> Stack(object item)
        {
            return base.Stack(item) as ItemWithPropertyEventArgs<ItemType>;
        }
    }
    public class ItemWithPropertyEventArgs : ItemEventArgs
    {
        public PropertyInfo Property { get; protected set; }
        public ItemWithPropertyEventArgs(Item item, PropertyInfo property) : base(item)
        {
            this.Property = property;
        }
        public new ItemWithPropertyEventArgs Stack(object item)
        {
            return base.Stack(item) as ItemWithPropertyEventArgs;
        }
    }
    public class ItemEventArgs : InvokeHistoyEventArgs
    {
        public Item Item { get; protected set; }
        public ItemEventArgs(Item item)
        {
            Item = item;
        }
        public new ItemEventArgs Stack(object item)
        {
            return base.Stack(item) as ItemEventArgs;
        }
    }
    public interface IItemEventArgs<out ItemType> where ItemType : Item { }
    public class ItemEventArgs<ItemType> : ItemEventArgs, IItemEventArgs<ItemType> where ItemType : Item
    {
        public new ItemType Item { get => base.Item as ItemType; private set => base.Item = value; }
        public ItemEventArgs(ItemType item):base(item) {}
        public ItemEventArgs(ItemEventArgs args):base(args.Item)
        {
            InvokeHistory = args.InvokeHistory;
        }
    }
    public class ChildrenSwappedEventArgs : InvokeHistoyEventArgs
    {
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }
        public ChildrenSwappedEventArgs(int oldIndex, int newIndex)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
        public new ChildrenSwappedEventArgs Stack(object item)
        {
            return base.Stack(item) as ChildrenSwappedEventArgs;
        }
    }
    public class InvokeHistoyEventArgs : EventArgs
    {
        public Stack<object> InvokeHistory { get; set; } = new Stack<object>();
        public virtual InvokeHistoyEventArgs Stack(object item)
        {
            InvokeHistory.Push(item);
            return this;
        }
    }
}
