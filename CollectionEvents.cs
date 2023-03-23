namespace CascadeEventFramework
{
    public delegate void CovariantEventHandler<out ItemType>(object sender, Item eventArgs) where ItemType : Item;
    public interface ICollectionEvents
    {
        event EventHandler<ItemEventArgs> ItemAdded;
        event EventHandler<ItemEventArgs> ItemRemoved;
        event EventHandler<ItemEventArgs> BeforeItemUpdated;
        event EventHandler<ItemEventArgs> ItemUpdated;
        event EventHandler<ChildrenSwappedEventArgs> ItemPositionChanged;
    }
    public class CollectionEvents<ItemType> where ItemType : Item
    {

        public event EventHandler<ItemEventArgs<ItemType>> ItemAdded;
        public event EventHandler<ItemEventArgs<ItemType>> ItemRemoved;
        public event EventHandler<ItemEventArgs<ItemType>> BeforeItemUpdated;
        public event EventHandler<ItemEventArgs<ItemType>> ItemUpdated;
        public CollectionEvents(CollectionEvents holder)
        {
            holder.ItemAdded += (s, e) => ItemAdded?.Invoke(s, new ItemEventArgs<ItemType>(e));
            holder.ItemRemoved += (s, e) => ItemRemoved?.Invoke(s, new ItemEventArgs<ItemType>(e));
            holder.BeforeItemUpdated += (s, e) => BeforeItemUpdated?.Invoke(s, new ItemEventArgs<ItemType>(e));
            holder.ItemUpdated += (s, e) => ItemUpdated?.Invoke(s, new ItemEventArgs<ItemType>(e));
        }
    }
    public class CollectionEvents : ICollectionEvents
    {
        public virtual Item Holder { get; }

        public CollectionEvents(Item holder)
        {
            Holder = holder;
        }
        public virtual event EventHandler<ItemEventArgs> ItemAdded;
        public virtual event EventHandler<ItemEventArgs> ItemRemoved;
        public virtual event EventHandler<ItemEventArgs> BeforeItemUpdated;
        public virtual event EventHandler<ItemEventArgs> ItemUpdated;
        public virtual event EventHandler<ItemEventArgs> ActiveSubitemChanged;
        public virtual event EventHandler<ChildrenSwappedEventArgs> ItemPositionChanged;
        public void SubscribeToEvents(ICollectionEvents events)
        {
            events.ItemAdded += Events_SubitemAdded;
            events.ItemRemoved += Events_SubitemRemoved;
            events.ItemUpdated += Events_SubitemUpdated;
            events.BeforeItemUpdated += Events_BeforeSubitemUpdated;
            events.ItemPositionChanged += Events_SubitemPositionChanged;
        }
        public void UnsubscribeFromEvents(ICollectionEvents events)
        {
            events.ItemAdded -= Events_SubitemAdded;
            events.ItemRemoved -= Events_SubitemRemoved;
            events.ItemUpdated -= Events_SubitemUpdated;
            events.BeforeItemUpdated -= Events_BeforeSubitemUpdated;
            events.ItemPositionChanged -= Events_SubitemPositionChanged;
        }
        private void Events_SubitemPositionChanged(object sender, ChildrenSwappedEventArgs e) => ItemPositionChanged?.Invoke(sender, e.Stack(Holder));
        private void Events_ActiveSubitemChanged(object sender, ItemEventArgs e) => ActiveSubitemChanged?.Invoke(sender, e.Stack(Holder));
        private void Events_BeforeSubitemUpdated(object sender, ItemEventArgs e) => BeforeItemUpdated?.Invoke(sender, e.Stack(Holder));
        private void Events_SubitemUpdated(object sender, ItemEventArgs e) => ItemUpdated?.Invoke(sender, e.Stack(Holder));
        private void Events_SubitemRemoved(object sender, ItemEventArgs e) => ItemRemoved?.Invoke(sender, e.Stack(Holder));
        private void Events_SubitemAdded(object sender, ItemEventArgs e) => ItemAdded?.Invoke(sender, e.Stack(Holder));
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
