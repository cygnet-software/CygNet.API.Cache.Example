using CygNet.API.Cache;
using CygNet.API.Core;
using CygNet.Data.Core;
using CygNet.Data.Points;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CygNet.Data.Core.CygNetCoreProperties;

namespace CygNet.API.Cache.Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region PROPERTIES

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private CoreCache _coreCache;

        // any property in the CygNetProperty enumeration can be added to this list
        private PropertyIdList _properties = new PropertyIdList()
        {
            CygNetProperty.Value,
            CygNetProperty.FacilityDescription,
            CygNetProperty.GeneralData1
        };

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        private string _general1;
        public string General1
        {
            get => _general1;
            set
            {
                _general1 = value;
                OnPropertyChanged("General1");
            }
        }

        private string _facilityDescription;
        public string FacilityDescription
        {
            get => _facilityDescription;
            set
            {
                _facilityDescription = value;
                OnPropertyChanged("FacilityDescription");
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            _coreCache = new CoreCache(new ServiceInformation().GetAmbientDomain());

            PointTagTextBox.Text = "[Enter a Point Tag here]";
        }

        private CoreCacheTag GetCoreCacheTag()
        {
            var pointTag = new PointTag(PointTagTextBox.Text);
            var cacheTag = new CoreCacheTag(pointTag);

            return cacheTag;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _coreCache.GetPropertyValue(GetCoreCacheTag(), _properties, out PropertyDataBlockList values, out List<string> errors);
            ProcessCacheResults(values);
        }

        private void ProcessCacheResults(PropertyDataBlockList values)
        {
            try
            {
                // the cache will try to respond as quickly as it can. sometimes that means that some requested information
                // will be missing. for example, the underlying facility cache can take up to 5 seconds to get updated
                // with a new record, so facility information might be missing from a GetPropertyValue() request. updates will
                // always be sent as they are received

                if (values.Any(x => x.PropertyId == CygNetProperty.Value))
                {
                    var value = values.First(x => x.PropertyId == CygNetProperty.Value).StringValue;
                    Value = value.Trim();
                }

                if (values.Any(x => x.PropertyId == CygNetProperty.FacilityDescription))
                {
                    var facilityDescription = values.First(x => x.PropertyId == CygNetProperty.FacilityDescription).StringValue;
                    FacilityDescription = facilityDescription;
                }

                if (values.Any(x => x.PropertyId == CygNetProperty.GeneralData1))
                {
                    var general1 = values.First(x => x.PropertyId == CygNetProperty.GeneralData1).StringValue;
                    General1 = general1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _coreCache.Subscribe(GetCoreCacheTag(), _properties, OnUpdate);
        }

        private void OnUpdate(object sender, CoreCacheUpdateEventArgs e)
        {
            ProcessCacheResults(e.PropertyDataBlockList);
        }
    }
}
