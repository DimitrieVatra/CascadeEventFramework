using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace CascadeEventFramework
{
    public abstract class Item<ParentType> : Item where ParentType : Item
    {
        public ParentType Parent { get; set; }
    }
    public abstract class Item : IItemEvents, INotifyPropertyChanged
    {
        protected Item()
        {
            InitializeSubEventsDictionary();
            CollectionsEvents.CollectionChanged += CollectionEvents_ObservableDictionaryChanged;
            PropertiesEvents.CollectionChanged += PropertiesEvents_ObservableDictionaryChanged;
        }
        public event EventHandler<KeyValuePair<Type, CollectionEvents>> CollectionEventsTypeAdded;
        public event EventHandler<KeyValuePair<Type, PropertyEvents>> PropertyEventsTypeAdded;


        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<ItemWithPropertyEventArgs> Updated;
        public event EventHandler<BeforeUpdatedEventArgs> BeforeUpdated;
        public ObservableDictionary<Type, CollectionEvents> CollectionsEvents { get; protected set; } = new ObservableDictionary<Type, CollectionEvents>();
        public ObservableDictionary<Type, PropertyEvents> PropertiesEvents { get; protected set; } = new ObservableDictionary<Type, PropertyEvents> { };
        protected virtual void OnPropertyChanged(PropertyInfo propertyInfo)
        {
            var type = propertyInfo.PropertyType;
            if (type.IsSubclassOf(typeof(Item)))
            {
                var propery = propertyInfo.GetValue(this, null) as Item;
                if (propery != null)
                    SubscribeToProperty(propery);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyInfo.Name));
            Updated?.Invoke(this, new ItemWithPropertyEventArgs(this, propertyInfo));
        }
        protected virtual void OnBeforeUpdated(PropertyInfo propertyInfo)
        {
            BeforeUpdated?.Invoke(this, new BeforeUpdatedEventArgs(this, propertyInfo));
            var type = propertyInfo.PropertyType;
            if (type.IsSubclassOf(typeof(Item)))
            {
                var propery = propertyInfo.GetValue(this, null) as Item;
                if (propery != null)
                    UnsubscribeFromProperty(propery);//unsubscribe from the previousvalue
            }
        }
        protected void InitializeSubEventsDictionary()
        {
            foreach (var property in GetType().GetProperties().Where(p => p.PropertyType.IsSubclassOf(typeof(Item))))
            {
                if (property.Name == nameof(Item<Item>.Parent))
                    continue;
                PropertiesEvents[property.PropertyType] = new PropertyEvents(this);
            }
            foreach (var property in GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Collection<,>))))
            {
                var templateType = property.PropertyType.GetGenericArguments()[0];
                CollectionsEvents[templateType] = new CollectionEvents(this);
            }
            foreach (var property in GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(ItemEvents<>))))
            {
                var templateType = property.PropertyType.GetGenericArguments()[0];
                GetOrCreatePropertyEvents(templateType);
            }
            foreach (var property in GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(CollectionEvents<>))))
            {
                var templateType = property.PropertyType.GetGenericArguments()[0];
                GetOrCreateColelctionEvents(templateType);
            }
        }

        private PropertyEvents GetOrCreatePropertyEvents(Type templateType)
        {
            if (!PropertiesEvents.TryGetValue(templateType, out PropertyEvents value))
                value = PropertiesEvents[templateType] = new PropertyEvents(this);
            return value;
        }
        private CollectionEvents GetOrCreateColelctionEvents(Type templateType)
        {
            if (!CollectionsEvents.TryGetValue(templateType, out CollectionEvents value))
                value = CollectionsEvents[templateType] = new CollectionEvents(this);
            return value;
        }

        internal void SubscribeToCollection<ItemType, ParentType>(Collection<ItemType, ParentType> collection) where ItemType : Item<ParentType> where ParentType : Item
        {
            CollectionsEvents.TryGetValue(typeof(ItemType), out CollectionEvents subitemsEvents);
            if (subitemsEvents is null)
            {
                subitemsEvents = new CollectionEvents(this);
                CollectionsEvents.Add(typeof(ItemType), subitemsEvents);
            }
            subitemsEvents.SubscribeToEvents(collection);
            collection.Items.ToList().ForEach(i => SubscribeToItem(i));

            collection.ItemAdded += (s, e) => SubscribeToItem(e.Item);
            collection.ItemRemoved += (s, e) => UnsubscribeFromItem(e.Item);
        }
        protected void SubscribeToItem(Item item)
        {
            SubscribeToCollectionEvents(item);
            SubscribeToPropertiesEvents(item);
            SubscribeToItemDictionary(item);
        }

        private void SubscribeToItemDictionary(Item item)
        {
            item.CollectionEventsTypeAdded += Item_CollectionEventsTypeAdded;
            item.PropertyEventsTypeAdded += Item_PropertyEventsTypeAdded;
        }
        private void UnsubscribeFromItemDictionary(Item item)
        {
            item.CollectionEventsTypeAdded -= Item_CollectionEventsTypeAdded;
            item.PropertyEventsTypeAdded -= Item_PropertyEventsTypeAdded;
        }
        private void Item_PropertyEventsTypeAdded(object sender, KeyValuePair<Type, PropertyEvents> e)
        {
            var propertyEvents = GetOrCreatePropertyEvents(e.Key);
            propertyEvents.SubscribeToEvents(e.Value);
        }
        private void Item_CollectionEventsTypeAdded(object sender, KeyValuePair<Type, CollectionEvents> e)
        {
            var collectionEvents = GetOrCreateColelctionEvents(e.Key);
            collectionEvents.SubscribeToEvents(e.Value);
        }

        private void SubscribeToCollectionEvents(Item item)
        {
            item.CollectionsEvents.ToList().ForEach(ev =>
            {
                if (!CollectionsEvents.TryGetValue(ev.Key, out CollectionEvents evValue))
                    evValue = CollectionsEvents[ev.Key] = new CollectionEvents(this);
                evValue.SubscribeToEvents(ev.Value);
            });
        }
        protected void SubscribeToProperty(Item item)
        {
            SubscribeToItem(item);
            if (!PropertiesEvents.TryGetValue(item.GetType(), out PropertyEvents evItem))
                evItem = PropertiesEvents[item.GetType()] = new PropertyEvents(this);
            evItem.SubscribeToEvents(item);
        }
        protected void UnsubscribeFromProperty(Item item)
        {
            UnsubscribeFromItem(item);
            if (PropertiesEvents.TryGetValue(item.GetType(), out PropertyEvents evValue))
                evValue.UnsubscribeFromEvents(item);
            UnsubscribeFromSubpropertiesEvents(item);
        }
        private void SubscribeToPropertiesEvents(Item item)
        {
            item.PropertiesEvents.ToList().ForEach(ev =>
            {
                if (!PropertiesEvents.TryGetValue(ev.Key, out PropertyEvents evValue))
                    evValue = PropertiesEvents[ev.Key] = new PropertyEvents(this);
                evValue.SubscribeToEvents(ev.Value);
            });
        }

        private void UnsubscribeFromSubpropertiesEvents(Item item)
        {
            item.PropertiesEvents.ToList().ForEach(ev =>
            {
                if (PropertiesEvents.TryGetValue(ev.Key, out PropertyEvents evValue))
                    evValue.UnsubscribeFromEvents(ev.Value);
            });
        }

        private void UnsubscribeFromItem(Item item)
        {
            UnsubscribeFromItemDictionary(item);
            item.CollectionsEvents.ToList().ForEach(ev =>
            {
                if (CollectionsEvents.TryGetValue(ev.Key, out CollectionEvents evValue))
                    evValue.UnsubscribeFromEvents(ev.Value);
            });
        }
        private void PropertiesEvents_ObservableDictionaryChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (KeyValuePair<Type, PropertyEvents> newItem in e.NewItems)
                {
                    PropertyEventsTypeAdded?.Invoke(this, newItem);
                }
            }
        }
        private void CollectionEvents_ObservableDictionaryChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (KeyValuePair<Type, CollectionEvents> newItem in e.NewItems)
            {
                CollectionEventsTypeAdded?.Invoke(this, newItem);
            }
        }
        protected bool SetField<T>(ref T field, T value, PropertyInfo propertyInfo)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            OnBeforeUpdated(propertyInfo);
            field = value;
            OnPropertyChanged(propertyInfo);
            return true;
        }
        protected bool SetField<T>(ref T field, T value, string propertyName) => SetField(ref field, value, GetType().GetProperty(propertyName));

        private int _index;

        public int Index
        {
            get { return _index; }
            set { SetField(ref _index, value, nameof(Index)); }
        }
    }
}
