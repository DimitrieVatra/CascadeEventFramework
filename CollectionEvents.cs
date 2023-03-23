namespace CascadeEventFramework
{
    public delegate void CovariantEventHandler<out ItemType>(object sender, Item eventArgs) where ItemType : Item;
    public interface ICollectionEvents
    {
        event EventHandler<ItemEventArgs> SubitemAdded;
        event EventHandler<ItemEventArgs> SubitemRemoved;
        event EventHandler<ItemEventArgs> BeforeSubitemUpdated;
        event EventHandler<ItemEventArgs> SubitemUpdated;
        event EventHandler<ChildrenSwappedEventArgs> SubitemPositionChanged;
    }
    public class CollectionEvents<ItemType> where ItemType : Item
    {

        public event EventHandler<ItemEventArgs<ItemType>> SubitemAdded;
        public event EventHandler<ItemEventArgs<ItemType>> SubitemRemoved;
        public event EventHandler<ItemEventArgs<ItemType>> BeforeSubitemUpdated;
        public event EventHandler<ItemEventArgs<ItemType>> SubitemUpdated;
        public CollectionEvents(CollectionEvents holder)
        {
            holder.SubitemAdded += (s, e) => SubitemAdded?.Invoke(s, new ItemEventArgs<ItemType>(e));
            holder.SubitemRemoved += (s, e) => SubitemRemoved?.Invoke(s, new ItemEventArgs<ItemType>(e));
            holder.BeforeSubitemUpdated += (s, e) => BeforeSubitemUpdated?.Invoke(s, new ItemEventArgs<ItemType>(e));
            holder.SubitemUpdated += (s, e) => SubitemUpdated?.Invoke(s, new ItemEventArgs<ItemType>(e));
        }
    }
    public class CollectionEvents : ICollectionEvents
    {
        public virtual Item Holder { get; }

        public CollectionEvents(Item holder)
        {
            Holder = holder;
        }
        public virtual event EventHandler<ItemEventArgs> SubitemAdded;
        public virtual event EventHandler<ItemEventArgs> SubitemRemoved;
        public virtual event EventHandler<ItemEventArgs> BeforeSubitemUpdated;
        public virtual event EventHandler<ItemEventArgs> SubitemUpdated;
        public virtual event EventHandler<ItemEventArgs> ActiveSubitemChanged;
        public virtual event EventHandler<ChildrenSwappedEventArgs> SubitemPositionChanged;
        public void SubscribeToEvents(ICollectionEvents events)
        {
            events.SubitemAdded += Events_SubitemAdded;
            events.SubitemRemoved += Events_SubitemRemoved;
            events.SubitemUpdated += Events_SubitemUpdated;
            events.BeforeSubitemUpdated += Events_BeforeSubitemUpdated;
            events.SubitemPositionChanged += Events_SubitemPositionChanged;
        }
        public void UnsubscribeFromEvents(ICollectionEvents events)
        {
            events.SubitemAdded -= Events_SubitemAdded;
            events.SubitemRemoved -= Events_SubitemRemoved;
            events.SubitemUpdated -= Events_SubitemUpdated;
            events.BeforeSubitemUpdated -= Events_BeforeSubitemUpdated;
            events.SubitemPositionChanged -= Events_SubitemPositionChanged;
        }
        private void Events_SubitemPositionChanged(object sender, ChildrenSwappedEventArgs e) => SubitemPositionChanged?.Invoke(sender, e.Stack(Holder));
        private void Events_ActiveSubitemChanged(object sender, ItemEventArgs e) => ActiveSubitemChanged?.Invoke(sender, e.Stack(Holder));
        private void Events_BeforeSubitemUpdated(object sender, ItemEventArgs e) => BeforeSubitemUpdated?.Invoke(sender, e.Stack(Holder));
        private void Events_SubitemUpdated(object sender, ItemEventArgs e) => SubitemUpdated?.Invoke(sender, e.Stack(Holder));
        private void Events_SubitemRemoved(object sender, ItemEventArgs e) => SubitemRemoved?.Invoke(sender, e.Stack(Holder));
        private void Events_SubitemAdded(object sender, ItemEventArgs e) => SubitemAdded?.Invoke(sender, e.Stack(Holder));
    }
    public interface IItemEvents
    {
        event EventHandler<ItemWithPropertyEventArgs> Updated;
        event EventHandler<BeforeUpdatedEventArgs> BeforeUpdated;
    }
    public class ItemEvents<ItemType> where ItemType : Item
    {
        public event EventHandler<ItemWithPropertyEventArgs<ItemType>> Updated;
        public event EventHandler<BeforeUpdatedEventArgs<ItemType>> BeforeUpdated;
        public ItemEvents(PropertyEvents holder)
        {
            holder.Updated += (s, e) => Updated?.Invoke(s, new ItemWithPropertyEventArgs<ItemType>(e));
            holder.BeforeUpdated += (s, e) => BeforeUpdated?.Invoke(s, new BeforeUpdatedEventArgs<ItemType>(e));
        }
    }
    public class PropertyEvents : IItemEvents
    {
        public virtual Item Holder { get; }

        public PropertyEvents(Item holder)
        {
            Holder = holder;
        }
        public virtual event EventHandler<ItemWithPropertyEventArgs> Updated;
        public virtual event EventHandler<BeforeUpdatedEventArgs> BeforeUpdated;
        public void SubscribeToEvents(IItemEvents events)
        {
            events.Updated += Events_PropertyChanged;
            events.BeforeUpdated += Events_BeforeUpdated;
        }
        public void UnsubscribeFromEvents(IItemEvents events)
        {
            events.Updated -= Events_PropertyChanged;
            events.BeforeUpdated -= Events_BeforeUpdated;
        }
        private void Events_PropertyChanged(object sender, ItemWithPropertyEventArgs e) => Updated?.Invoke(sender, e.Stack(Holder));
        private void Events_BeforeUpdated(object sender, BeforeUpdatedEventArgs e) => BeforeUpdated?.Invoke(sender, e.Stack(Holder));
    }
}
