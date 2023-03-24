# EventCascadeFramework
Automatically propagate events from children to parent without the need to subscribe and unsubscribe upon collections or properties change. The cascading is automatically updated.

Use example:
University contains Classes, A class contains Courses, a Course contains Lessons. The university has an active Class, an active Course which is the Active course of the active class, and an active lesson which is the active lesson of the active course of the active class.

```csharp
public class University
{
    public string Name { get; set; }
    public Class ActiveClass { get; set; }
    public Lesson ActiveLesson { get; set; }// = ActiveClass.ActiveCourse.ActiveLesson whic is difficult to maintain
    public event EventHandler<Lesson> ActiveLessonChanged;//Difficult to maintain, we need to keep the track of the ActiveClass changed and ActiveCourse changed
    public List<Class> Classes{ get; set; }
}
public class Class
{
    public string Name { get; set; }
    public Course ActiveCourse { get ; set; }
    public List<Course> Courses { get; set; }
}
public class Course
{
    public string Name { get; set; }
    public Lesson ActiveLesson { get; set; }
    public List<Lesson> Lessons { get; set; }
}
public class Lesson
{
    public string Name { get; set; }
}

```
To maintain the University.ActiveLesson updated, you would need to subscribe to the entire hierachy of the model, and propagate the events and implement very much logic around it.

## Solution:
No more subscribing and unsubscribing to events. It can lead to bugs and many errors. Instead, CascadeEventFramework automatically does the event binding for you, and updates it upon every collection and item changes.

1. None of the intermediate classes need to subscribe to anything, the cascading is maintained automatically
```csharp
public class Class : Item<University>
{
    string _name;
    Course _activeCourse;
    public string Name { get => _name; set => SetField(ref _name, value, nameof(Name)); }
    public Course ActiveCourse { get => _activeCourse; set => SetField(ref _activeCourse, value, nameof(ActiveCourse)); }
    public Collection<Course, Class> Courses { get; set; }
    public Class()
    {
        Courses = new Collection<Course, Class>(this);
    }
}
public class Course : Item<Class>
{
    string _name;
    Course _activeCourse;
    public string Name { get => _name; set => SetField(ref _name, value, nameof(Name)); }
    public Course ActiveCourse { get => _activeCourse; set => SetField(ref _activeCourse, value, nameof(ActiveCourse)); }
    public Collection<Lesson, Course> Lessons { get; set; }
    public Course()
    {
        Lessons = new Collection<Lesson, Course>(this);
    }
}
public class Lesson : Item<Course>
{
    string _name;
    public string Name
    {
        get => _name; set => SetField(ref _name, value, nameof(Name));
    }
}
```

1. Where you want to extract an event, just create a class of type CollectionEvents (for events coming from a sub-collection in the hierarchy) or ItemEvents (for events comming from a sub-property in the hierarchy). And they will be automatically invoked when needed:
```csharp
public class University : Item
{
    public Lesson ActiveLesson { get;set; }
    public Collection<Class, University> Classes { get; }
    public CollectionEvents<Class> ClassesEvents { get; }
    public CollectionEvents<Lesson> LessonsEvents { get; }
    public ItemEvents<Lesson> ActiveLessonEvents { get; }

    public University()
    {
        Classes = new Collection<Class, University>(this);

        //Declare here only the cascade event trackers you are interested into, and ignore all the rest. For example if you don'y need to know when the Courses collection is being changed, you don's need to refference that one, but you can still follow events of sub-items of courses
        ClassesEvents = new CollectionEvents<Class>(CollectionsEvents[typeof(Class)]);
        LessonsEvents = new CollectionEvents<Lesson>(CollectionsEvents[typeof(Lesson)]);
        ActiveLessonEvents = new ItemEvents<Lesson>(PropertiesEvents[typeof(Lesson)]);

        //Fires for all the objects in the Classes collection. When an object is removed from the colelction, this event will not folow it anymore. Also newly added objects will be included in this event
        ClassesEvents.ItemUpdated += ClassesEvents_SubitemUpdated;

        //Fires for all the lessons in all the courses in the Classes collection. No need to subscribe and unsubscribe to and from each Course of each Class upon every collection change
        LessonsEvents.ItemUpdated += LessonsEvents_ItemUpdated;

        //Fires Automatically for the ActiveClass.ActiveCourse.ActiveLesson property. When the ActiveClass or ActiveCourse or ActiveLesson changes, the event will follow the updated refference
        ActiveLessonEvents.Updated += ActiveLessonEvents_Updated;
    }

    private void LessonsEvents_ItemUpdated(object sender, ItemWithPropertyEventArgs<Lesson> e)
    {
        Debug.WriteLine($"The lesson {e.Item.Name} of the class {e.Item.Parent.Name} of the class {e.Item.Parent.Parent.Name} from the collection Classes has been updated");
        Debug.WriteLine($"Updated property: {e.Property.Name})");
    }

    private void ClassesEvents_SubitemUpdated(object sender, ItemWithPropertyEventArgs<Class> e)
    {
        Debug.WriteLine($"The class {e.Item.Name} from the collection Classes has been updated");
        Debug.WriteLine($"Updated property: {e.Property.Name})");
    }
    private void ActiveLessonEvents_Updated(object sender, ItemWithPropertyEventArgs<Lesson> e) => ActiveLesson = e.Item;
}

```




## Classical implementation
  
Following there is a classical implementation of the model above, but I eliminated some of the functionality to make it smaller, as it would have been too long:
<details>
  <summary>Expand here to check the classical equivalent of the code above</summary>

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;

public class University : INotifyPropertyChanged
{
    private string _name;
    private Class _activeClass;
    private Lesson _activeLesson;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public Class ActiveClass
    {
        get => _activeClass;
        set
        {
            if (_activeClass != value)
            {
                if (_activeClass != null)
                {
                    _activeClass.PropertyChanged -= ActiveClass_PropertyChanged;
                }

                _activeClass = value;
                _activeClass.PropertyChanged += ActiveClass_PropertyChanged;
                UpdateActiveLesson();
            }
        }
    }

    public Lesson ActiveLesson
    {
        get => _activeLesson;
        set
        {
            if (_activeLesson != value)
            {
                if (_activeLesson != null)
                {
                    _activeLesson.PropertyChanged -= ActiveLesson_PropertyChanged;
                }

                _activeLesson = value;

                if (_activeLesson != null)
                {
                    _activeLesson.PropertyChanged += ActiveLesson_PropertyChanged;
                }

                ActiveLessonChanged?.Invoke(this, _activeLesson);
            }
        }
    }

    public event EventHandler<Lesson> ActiveLessonChanged;
    public event EventHandler<Class> ActiveClassRenamed;
    public event EventHandler<Course> ActiveCourseRenamed;
    public event EventHandler<Lesson> LessonRenamed;
    public List<Class> Classes { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    private void ActiveClass_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Class.ActiveCourse))
        {
            UpdateActiveLesson();
        }
        else if (e.PropertyName == nameof(Class.Name))
        {
            ActiveClassRenamed?.Invoke(this, _activeClass);
        }
    }

    private void UpdateActiveLesson()
    {
        ActiveLesson = _activeClass?.ActiveCourse?.ActiveLesson;
    }

    private void ActiveLesson_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Lesson.Name))
        {
            LessonRenamed?.Invoke(this, _activeLesson);
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Class : INotifyPropertyChanged
{
    private string _name;
    private Course _activeCourse;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public Course ActiveCourse
    {
        get => _activeCourse;
        set
        {
            if (_activeCourse != value)
            {
                if (_activeCourse != null)
                {
                    _activeCourse.PropertyChanged -= ActiveCourse_PropertyChanged;
                }

                _activeCourse = value;

                if (_activeCourse != null)
                {
                    _activeCourse.PropertyChanged += ActiveCourse_PropertyChanged;
                }

                OnPropertyChanged(nameof(ActiveCourse));
            }
        }
    }

    public List<Course> Courses { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    private void ActiveCourse_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Course.ActiveLesson))
        {
            OnPropertyChanged(nameof(ActiveCourse));
        }
        else if (e.PropertyName == nameof(Course.Name))
        {
            OnPropertyChanged(nameof(Course.Name));
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Course : INotifyPropertyChanged
{
    private string _name;
    private Lesson _activeLesson;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public Lesson ActiveLesson
    {
        get => _activeLesson;
        set
        {
            if (_activeLesson != value)
            {
                _activeLesson = value;
                OnPropertyChanged(nameof(ActiveLesson));
            }
        }
    }

    public List<Lesson> Lessons { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Lesson : INotifyPropertyChanged
{
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


```

</details>
