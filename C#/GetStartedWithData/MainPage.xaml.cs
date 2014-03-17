using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

//// TODO: Add the following using statement.
//using Microsoft.WindowsAzure.MobileServices;
//using Newtonsoft.Json;

namespace GetStartedWithData
{    
    public class TodoItem
    {
        public string Id { get; set; }

        //// TODO: Add the following serialization attribute.
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        //// TODO: Add the following serialization attribute.
        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }
    }

    
    public sealed partial class MainPage : Page
    {
		// TODO: Comment out the following line that defined the in-memory collection.
        //private ObservableCollection<Item> items = new ObservableCollection<Item>();

        //// TODO: Uncomment the following two lines of code to replace the following collection with todoTable, 
        //// a proxy for the table in SQL Database.
        private MobileServiceCollection<TodoItem, TodoItem> items;
        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
        DispatcherTimer t = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            t.Interval = new TimeSpan(0, 0, 10);
            t.Tick += t_Tick;
            t.Start();
        }

        async void t_Tick(object sender, object e)
        {
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var mediaCaptureMgr = new MediaCapture();
            await mediaCaptureMgr.InitializeAsync();
            mediaCaptureMgr.SetPreviewMirroring(true);
            await mediaCaptureMgr.CapturePhotoToStreamAsync(imageProperties, memStream);
            await memStream.FlushAsync();
            memStream.Seek(0);
            WriteableBitmap wb1 = new WriteableBitmap(320, 240);
            await wb1.SetSourceAsync(memStream);
            ImgCapture.Source = wb1;
            t.Stop();
        }

        private async void InsertItem(TodoItem Item)
        {
            // TODO: Delete or comment the following statement; Mobile Services auto-generates the ID.
            //       You can leave this if you want to generate your own unique id values instead.
            Item.Id = Guid.NewGuid().ToString();

            //// This code inserts a new Item into the database. When the operation completes
            //// and Mobile Services has assigned an Id, the item is added to the CollectionView
            //// TODO: Mark this method as "async" and uncomment the following statement.
            await todoTable.InsertAsync(Item);

             
            items.Add(Item);
        }

        private async void RefreshItems()
        {
            //// TODO #1: Mark this method as "async" and uncomment the following statment
            //// that defines a simple query for all items. 
            items = await todoTable.ToCollectionAsync();

            //// TODO #2: More advanced query that filters out completed items. 
            items = await todoTable
               .Where(Item => Item.Complete == false)
               .ToCollectionAsync();
           
            ListItems.ItemsSource = items;
        }

        private async void UpdateCheckedItem(TodoItem item)
        {
            //// This code takes a freshly completed Item and updates the database. When the MobileService 
            //// responds, the item is removed from the list.
            //// TODO: Mark this method as "async" and uncomment the following statement
             await todoTable.UpdateAsync(item);     
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshItems();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var Item = new TodoItem { Text = TextInput.Text };
            InsertItem(Item);
        }

        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            UpdateCheckedItem(item);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshItems();
        }
    }
}
