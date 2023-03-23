using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace EventCascadeFramework
{
    public class Collection<ItemType, ParentType> : ICollectionEvents where ItemType : Item<ParentType> where ParentType : Item
    {
        public event EventHandler<ItemEventArgs> SubitemAdded;
        public event EventHandler<ItemEventArgs> SubitemRemoved;
        public event EventHandler<ItemEventArgs> BeforeSubitemUpdated;
        public event EventHandler<ItemEventArgs> SubitemUpdated;
        public event EventHandler<ChildrenSwappedEventArgs> SubitemPositionChanged;

        private ObservableCollection<ItemType> _items;
        ParentType Parent;
        public Collection(ParentType parent)
        {
            _items = new ObservableCollection<ItemType>();
            _items.CollectionChanged += OnCollectionChanged;
            Parent = parent;
            parent.SubscribeToCollection(this);
        }

        public ObservableCollection<ItemType> Items
        {
            get { return _items; }
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ItemType newItem in e.NewItems)
                {
                    newItem.Parent = Parent;
                    newItem.PropertyChanged += OnItemPropertyChanged;
                    newItem.BeforeUpdated += OnBeforeItemUpdated;

                    SubitemAdded?.Invoke(this, new ItemEventArgs(newItem));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ItemType oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= OnItemPropertyChanged;
                    oldItem.BeforeUpdated -= OnBeforeItemUpdated;
                    SubitemRemoved?.Invoke(this, new ItemEventArgs(oldItem));
                }
            }
        }
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemType item = (ItemType)sender;

            if (e.PropertyName == nameof(Item.Index))
            {
                ReorderItems(item);
            }
            else
            {
                SubitemUpdated?.Invoke(this, new ItemEventArgs(item));
            }
        }

        private void OnBeforeItemUpdated(object sender, BeforeUpdatedEventArgs e)
        {
            ItemType item = (ItemType)sender;
            BeforeSubitemUpdated?.Invoke(this, new ItemEventArgs(item));
        }

        private void ReorderItems(ItemType changedItem)
        {
            int oldIndex = Items.IndexOf(changedItem);
            Items.RemoveAt(oldIndex);
            int newIndex = Items.ToList().FindIndex(item => item.Index > changedItem.Index);
            if (newIndex == -1)
            {
                Items.Add(changedItem);
            }
            else
            {
                Items.Insert(newIndex, changedItem);
            }
            SubitemPositionChanged?.Invoke(this, new ChildrenSwappedEventArgs(oldIndex, newIndex));
        }
    }
}
