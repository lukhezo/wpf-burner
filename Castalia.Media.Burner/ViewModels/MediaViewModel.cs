using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Castalia.Mvvm;
using Castalia.Mvvm.ObjectServices;
using Castalia.Mvvm.Services.Core;
using IMAPI2.Interop;
using IMAPI2.MediaItem;


namespace Castalia.Media.Burner.ViewModels
{
    public class MediaViewModel : ViewModelBase
    {
        #region Private Fields
     
        private DelegateCommand addFilesCommand;
        private DelegateCommand addFoldersCommand;
        private DelegateCommand removeMediaItemsCommand;
        private DelegateCommand detectMediaCommand;
        private DelegateCommand burnCommand;
        private DelegateCommand formatCommand;
        private DelegateCommand ejectCommand;

        private const string ClientName = "Media Burner";

        Int64 totalDiscSize;

        private bool isBurning;
        private bool isFormatting;
        private IMAPI_BURN_VERIFICATION_LEVEL verificationLevel =
            IMAPI_BURN_VERIFICATION_LEVEL.IMAPI_BURN_VERIFICATION_NONE;
        private bool closeMedia;
        private bool ejectMedia;

        private readonly BurnData burnData = new BurnData();

        private BackgroundWorker backgroundBurnWorker;
        private BackgroundWorker backgroundFormatWorker;

        private ObservableCollection<IMediaItem> mediaItems;
       
        private ICollectionView discRecordersView;
        private ObservableCollection<MsftDiscRecorder2> discRecorders;

        private ICollectionView verificationTypesView;
        private ObservableCollection<string> verificationTypes;
      
        private IDiscRecorder2 currentRecorder;

        private string totalSize;
        private string volumeLabel;
        private string mediaType;
        private string burnStatusMsg;
        private string formatStatusMsg;
        private string supportedMedia;
        private int burnProgressValue;
        private int formatProgressValue;
        private int capacityProgressValue;
        private SolidColorBrush capacityProgressBrush;

        private bool shouldCloseMedia;
        private bool shouldEject;
        private bool shouldFormatQuick;
        private bool shouldEjectFormat;
 
        #endregion

        #region Private Properties

        private static bool IsInDesignMode
        {
            get
            {
                return (bool)DesignerProperties.IsInDesignModeProperty
                    .GetMetadata(typeof(DependencyObject)).DefaultValue;
            }
        }
        #endregion

        #region Public Properties


        /// <summary>
        /// The public AddFilesCommand ICommand
        /// </summary>
        public ICommand AddFilesCommand
        {
            get
            {
                return addFilesCommand ??
                       (addFilesCommand =
                        new DelegateCommand(param => ExecuteAddFilesCommand(), param => CanExecute));
            }
        }


        /// <summary>
        /// The public AddFoldersCommand ICommand
        /// </summary>
        public ICommand AddFoldersCommand
        {
            get
            {
                return addFoldersCommand ??
                       (addFoldersCommand =
                        new DelegateCommand(param => ExecuteAddFoldersCommand(), param => CanExecute));
            }
        }



        /// <summary>
        /// The public RemoveMediaItemsCommand ICommand
        /// </summary>
        public ICommand RemoveMediaItemsCommand
        {
            get
            {
                return removeMediaItemsCommand ??
                       (removeMediaItemsCommand =
                        new DelegateCommand(param => ExecuteRemoveMediaItemsCommand(), param => CanExecute));
            }
        }



        /// <summary>
        /// The public DetectMediaCommand ICommand
        /// </summary>
        public ICommand DetectMediaCommand
        {
            get
            {
                return detectMediaCommand ??
                       (detectMediaCommand =
                        new DelegateCommand(param => ExecuteDetectMediaCommand(), param => CanExecute));
            }
        }



        /// <summary>
        /// The public BurnCommand ICommand
        /// </summary>
        public ICommand BurnCommand
        {
            get
            {
                return burnCommand ??
                       (burnCommand =
                        new DelegateCommand(param => ExecuteBurnCommand(), param => CanBurn()));
            }
        }



        /// <summary>
        /// The public FormatCommand ICommand
        /// </summary>
        public ICommand FormatCommand
        {
            get
            {
                return formatCommand ??
                       (formatCommand =
                        new DelegateCommand(param => ExecuteFormatCommand(), param => CanExecute));
            }
        }


        /// <summary>
        /// The public EjectCommand ICommand
        /// </summary>
        public ICommand EjectCommand
        {
            get
            {
                return ejectCommand ??
                       (ejectCommand =
                        new DelegateCommand(param => ExecuteEjectCommand(), param => CanExecute));
            }
        }


        private bool canExecute = true;
        /// <summary>
        /// Whether or not this command can execute. At present this always need to be true
        /// </summary>
        public bool CanExecute
        {
            get { return canExecute; }
             set { canExecute = value; }
        }

        public string TotalSize
        {
            get { return totalSize; }
            set
            {
                totalSize = value;
                NotifyPropertyChanged("TotalSize");
            }
        }

        public string VolumeLabel
        {
            get { return volumeLabel; }
            set
            {
                volumeLabel = value;
                NotifyPropertyChanged("VolumeLabel");
            }
        }

        public ObservableCollection<IMediaItem> MediaItems
        {
            get { return mediaItems; }
            set
            {
                mediaItems = value;
                NotifyPropertyChanged("MediaItems");
            }

        }

        public ICollectionView DiscRecordersView
        {
            get
            {
                return discRecordersView;
            }
        }


        public ObservableCollection<MsftDiscRecorder2> DiscRecorders
        {
            get { return discRecorders; }
            set
            {
                discRecorders = value;
                NotifyPropertyChanged("DiscRecorders");
            }

        }
        
        public string MediaType
        {
            get { return mediaType; }
            set
            {
                mediaType = value;
                NotifyPropertyChanged("MediaType");

            }
        }

        public string BurnStatusMsg
        {
            get { return burnStatusMsg; }
            set
            {
                burnStatusMsg = value;
                NotifyPropertyChanged("BurnStatusMsg");

            }
        }

        public string FormatStatusMsg
        {
            get { return formatStatusMsg; }
            set
            {
                formatStatusMsg = value;
                NotifyPropertyChanged("FormatStatusMsg");

            }
        }

        public string SupportedMedia
        {
            get { return supportedMedia; }
            set
            {
                supportedMedia = value;
                NotifyPropertyChanged("SupportedMedia");

            }
        }

        public int BurnProgressValue
        {
            get { return burnProgressValue; }
            set
            {
                burnProgressValue = value;
                NotifyPropertyChanged("BurnProgressValue");

            }
        }

        public int FormatProgressValue
        {
            get { return formatProgressValue; }
            set
            {
                formatProgressValue = value;
                NotifyPropertyChanged("FormatProgressValue");

            }
        }

        public int CapacityProgressValue
        {
            get { return capacityProgressValue; }
            set
            {
                capacityProgressValue = value;
                NotifyPropertyChanged("CapacityProgressValue");

            }
        }

        public SolidColorBrush CapacityProgressBrush
        {
            get { return capacityProgressBrush; }
            set
            {
                capacityProgressBrush = value;
                NotifyPropertyChanged("CapacityProgressBrush");

            }
        }


        public ICollectionView VerificationTypesView
        {
            get
            {
                return verificationTypesView;
            }
        }



        public ObservableCollection<string> VerificationTypes
        {
            get { return verificationTypes; }
            set
            {
                verificationTypes = value;
                NotifyPropertyChanged("VerificationTypes");

            }
        }


        
        public bool ShouldCloseMedia
        {
            get { return shouldCloseMedia; }
            set
            {
                shouldCloseMedia = value;
                NotifyPropertyChanged("ShouldCloseMedia");

            }
        }

        public bool ShouldEject
        {
            get { return shouldEject; }
            set
            {
                shouldEject = value;
                NotifyPropertyChanged("ShouldEject");

            }
        }

        public bool ShouldFormatQuick
        {
            get { return shouldFormatQuick; }
            set
            {
                shouldFormatQuick = value;
                NotifyPropertyChanged("ShouldFormatQuick");

            }
        }
            
        public bool ShouldEjectFormat
        {
            get { return shouldEjectFormat; }
            set
            {
                shouldEjectFormat = value;
                NotifyPropertyChanged("ShouldEjectFormat");

            }
        }
        #endregion

        #region Constructor
     

        public MediaViewModel()
        {
            if (!IsInDesignMode)
            {
                //typically this would be in the App.cs in the main form so it can be accessed/
                // across all forms, but placed here since we have just the 1 view model
                ServiceInjector.InjectServices();

                InitialiseModel();

                AddRecordingDevices();
            }
        }

        private void InitialiseModel()
        {
            InitialiseBackgroundWorkers();

            var now = DateTime.Now;
            VolumeLabel =  now.Year + "_" + now.Month + "_" + now.Day;
            BurnStatusMsg = string.Empty;
            mediaItems = new ObservableCollection<IMediaItem>();
        
            verificationTypes = new ObservableCollection<string> {"None", "Quick", "Full"};
            verificationTypesView = CollectionViewSource.GetDefaultView(VerificationTypes);
            verificationTypesView.CurrentChanged += VerificationTypesViewCurrentChanged;
        }

        private void InitialiseBackgroundWorkers()
        {
            backgroundBurnWorker = new BackgroundWorker
                                       {
                                           WorkerReportsProgress = true,
                                           WorkerSupportsCancellation = true
                                       };
            backgroundBurnWorker.DoWork += BackgroundBurnWorkerDoWork;
            backgroundBurnWorker.ProgressChanged += BackgroundBurnWorkerProgressChanged;
            backgroundBurnWorker.RunWorkerCompleted += BackgroundBurnWorkerRunWorkerCompleted;

            backgroundFormatWorker = new BackgroundWorker
                                         {
                                             WorkerReportsProgress = true,
                                             WorkerSupportsCancellation = true
                                         };
            backgroundFormatWorker.DoWork += BackgroundFormatWorkerDoWork;
            backgroundFormatWorker.ProgressChanged += BackgroundFormatWorkerProgressChanged;
            backgroundFormatWorker.RunWorkerCompleted += BackgroundFormatWorkerRunWorkerCompleted;
        }

        void VerificationTypesViewCurrentChanged(object sender, EventArgs e)
        {
            verificationLevel = (IMAPI_BURN_VERIFICATION_LEVEL) verificationTypesView.CurrentPosition;
        }

        #endregion

        #region Private Methods

        private void AddRecordingDevices()
        {
            DiscRecorders = new ObservableCollection<MsftDiscRecorder2>();
            discRecordersView = CollectionViewSource.GetDefaultView(DiscRecorders);
            discRecordersView.CurrentChanged += OnDiscRecordersViewCurrentChanged;

            //
            // Determine the current recording devices
            //
            MsftDiscMaster2 discMaster = null;
            try
            {
                discMaster = new MsftDiscMaster2();

                if (!discMaster.IsSupportedEnvironment)
                    return;

                foreach (string uniqueRecorderId in discMaster)
                {
                    var discRecorder2 = new MsftDiscRecorder2();
                    discRecorder2.InitializeDiscRecorder(uniqueRecorderId);

                    DiscRecorders.Add(discRecorder2);
                }
                //// Dirty code
                //if (devicesComboBox.Items.Count > 0)
                //{
                //    devicesComboBox.SelectedIndex = 0;
                //}
            }
            catch (COMException ex)
            {
                var msgBox = GetService<IMessageBoxService>();

                if (msgBox != null)
                {
                    msgBox.Show(string.Format("Error:{0} - Please install IMAPI2", ex.ErrorCode),
                        "IMAPI2 error Changes", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                return;
            }
            finally
            {
                if (discMaster != null)
                {
                    Marshal.ReleaseComObject(discMaster);
                }
            }


            UpdateCapacity();
            CanBurn();
        }

 
        void OnDiscRecordersViewCurrentChanged(object sender, EventArgs e)
        {
            currentRecorder = discRecordersView.CurrentItem as IDiscRecorder2;

            if (currentRecorder != null)
            {
                SupportedMedia = string.Empty;

                //
                // Verify recorder is supported
                //
                IDiscFormat2Data discFormatData = null;
                try
                {
                    discFormatData = new MsftDiscFormat2Data();
                    if (!discFormatData.IsRecorderSupported(currentRecorder))
                    {
                        SupportedMedia = "Recorder not supported: " + ClientName;
                        return;
                    }

                    var supportedMediaTypes = new StringBuilder();

                    foreach (IMAPI_PROFILE_TYPE profileType in currentRecorder.SupportedProfiles)
                    {
                        string profileName = GetProfileTypeString(profileType);

                        if (string.IsNullOrEmpty(profileName))
                            continue;

                        if (supportedMediaTypes.Length > 0)
                            supportedMediaTypes.Append(", ");
                        supportedMediaTypes.Append(profileName);
                    }

                    SupportedMedia = supportedMediaTypes.ToString();
                    
                }
                catch (COMException)
                {
                    SupportedMedia = "Error getting supported types";
                }
                finally
                {
                    if (discFormatData != null)
                    {
                        Marshal.ReleaseComObject(discFormatData);
                    }
                }
            }
        }


        private void ExecuteAddFilesCommand()
        {
            var dialog = GetService<IOpenFileDialogService>();

            if (dialog != null)
            {
                var result = dialog.ShowDialog();

                if (result != null && (bool) result)
                {
                    foreach (var file in dialog.FileNames())
                    {
                        MediaItems.Add(new FileItem(file));
                    }

                    UpdateCapacity();
                    CanBurn();
                }
            }
        }
 
   

        private void ExecuteAddFoldersCommand()
        {
            var dialog = GetService<IFolderBrowserDialogService>();

            if (dialog != null)
            {
                var result = dialog.ShowDialog();

                if (result ==  DialogResult.OK)
                {
                    MediaItems.Add(new DirectoryItem(dialog.SelectedPath()));
                }

                UpdateCapacity();
                CanBurn();
                
            }
        }

        private void ExecuteRemoveMediaItemsCommand()
        {
            var deleteList = MediaItems.Where(m => m.IsSelected()).ToList();

            foreach (var item in deleteList)
            {
                MediaItems.Remove(item);
            }
        }




        private void ExecuteDetectMediaCommand()
        {
            if (currentRecorder != null)
            {

                MsftFileSystemImage fileSystemImage = null;
                MsftDiscFormat2Data discFormatData = null;

                try
                {
                    //
                    // Create and initialize the IDiscFormat2Data
                    //

                    discFormatData = new MsftDiscFormat2Data();
                    if (!discFormatData.IsCurrentMediaSupported(currentRecorder))
                    {
                        MediaType = "Media not supported";
                        totalDiscSize = 0;
                        return;
                    }
                    else
                    {
                        //
                        // Get the media type in the recorder
                        //
                        discFormatData.Recorder = currentRecorder;
                        IMAPI_MEDIA_PHYSICAL_TYPE currentPhysicalMediaType = discFormatData.CurrentPhysicalMediaType;
                        MediaType = GetMediaTypeString(currentPhysicalMediaType);

                        //
                        // Create a file system and select the media type
                        //
                        fileSystemImage = new MsftFileSystemImage();
                        fileSystemImage.ChooseImageDefaultsForMediaType(currentPhysicalMediaType);

                        //
                        // See if there are other recorded sessions on the disc
                        //
                        if (!discFormatData.MediaHeuristicallyBlank)
                        {
                            fileSystemImage.MultisessionInterfaces = discFormatData.MultisessionInterfaces;
                            fileSystemImage.ImportFileSystem();
                        }

                        Int64 freeMediaBlocks = fileSystemImage.FreeMediaBlocks;
                        totalDiscSize = 2048 * freeMediaBlocks;
                    }
                }
                catch (COMException exception)
                {
                    var msgBox = GetService<IMessageBoxService>();

                    msgBox?.Show(exception.Message, "Detect Media Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    if (discFormatData != null)
                    {
                        Marshal.ReleaseComObject(discFormatData);
                    }

                    if (fileSystemImage != null)
                    {
                        Marshal.ReleaseComObject(fileSystemImage);
                    }
                }


                UpdateCapacity();
                CanBurn();
            }
        }

        private void ExecuteBurnCommand()
        {
            //if (MediaItems.Count < 1 )
            //{
            //    return;
            //}

            if (isBurning)
            {
                //buttonBurn.IsEnabled = false;
                backgroundBurnWorker.CancelAsync();
            }
            else
            {
                isBurning = true;
                if (ShouldCloseMedia)
                {
                    closeMedia = ShouldCloseMedia;
                    if (ShouldEject) ejectMedia = ShouldEject;
                }

                EnableBurnUi(false);

                burnData.uniqueRecorderId = currentRecorder.ActiveDiscRecorder;
                backgroundBurnWorker.RunWorkerAsync(burnData);
            }
        }
     

        private void ExecuteFormatCommand()
        {
            if (currentRecorder != null)
            {
                isFormatting = true;
                EnableFormatUi(false);
                backgroundFormatWorker.RunWorkerAsync(currentRecorder.ActiveDiscRecorder);
            }
        }


        private void ExecuteEjectCommand()
        {
            if (currentRecorder != null)
            {
                try
                {
                    currentRecorder.EjectMedia();
                }
                catch (Exception ex)
                {
                    var msgBox = GetService<IMessageBoxService>();

                    if (msgBox != null)
                    {
                        msgBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    }
                }
            }
        }


        private  bool CanBurn()
        {
            if (!IsInDesignMode)
            {
                bool canburn = MediaItems.Count > 0;
                return canburn;
            }
            return false;
        }

        #endregion

        #region Device ComboBox

        //private void DevicesComboBoxSelectionChanged(object sender, RoutedEventArgs e)
        //{
        //    if (devicesComboBox.SelectedIndex == -1)
        //    {
        //        return;
        //    }

        //    var discRecorder =
        //        (IDiscRecorder2)devicesComboBox.Items[devicesComboBox.SelectedIndex];

        //    SupportedMedia = string.Empty;

        //    //
        //    // Verify recorder is supported
        //    //
        //    IDiscFormat2Data discFormatData = null;
        //    try
        //    {
        //        discFormatData = new MsftDiscFormat2Data();
        //        if (!discFormatData.IsRecorderSupported(discRecorder))
        //        {
        //            SupportedMedia = "Recorder not supported: " + ClientName;
        //            return;
        //        }

        //        var supportedMediaTypes = new StringBuilder();
        //        foreach (IMAPI_PROFILE_TYPE profileType in discRecorder.SupportedProfiles)
        //        {
        //            string profileName = GetProfileTypeString(profileType);

        //            if (string.IsNullOrEmpty(profileName))
        //                continue;

        //            if (supportedMediaTypes.Length > 0)
        //                supportedMediaTypes.Append(", ");
        //            supportedMediaTypes.Append(profileName);
        //        }

        //        SupportedMedia = supportedMediaTypes.ToString();
        //    }
        //    catch (COMException)
        //    {
        //        SupportedMedia = "Error getting supported types";
        //    }
        //    finally
        //    {
        //        if (discFormatData != null)
        //        {
        //            Marshal.ReleaseComObject(discFormatData);
        //        }
        //    }
        //}

        /// <summary>
        /// converts an IMAPI_MEDIA_PHYSICAL_TYPE to it's string
        /// </summary>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        private static string GetMediaTypeString(IMAPI_MEDIA_PHYSICAL_TYPE mediaType)
        {
            switch (mediaType)
            {
                default:
                    return "Unknown Media Type";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM:
                    return "CD-ROM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR:
                    return "CD-R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW:
                    return "CD-RW";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM:
                    return "DVD ROM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM:
                    return "DVD-RAM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR:
                    return "DVD+R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW:
                    return "DVD+RW";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER:
                    return "DVD+R Dual Layer";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR:
                    return "DVD-R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW:
                    return "DVD-RW";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER:
                    return "DVD-R Dual Layer";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK:
                    return "random-access writes";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER:
                    return "DVD+RW DL";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM:
                    return "HD DVD-ROM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR:
                    return "HD DVD-R";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM:
                    return "HD DVD-RAM";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM:
                    return "Blu-ray DVD (BD-ROM)";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR:
                    return "Blu-ray media";

                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE:
                    return "Blu-ray Rewritable media";
            }
        }

        /// <summary>
        /// converts an IMAPI_PROFILE_TYPE to it's string
        /// </summary>
        /// <param name="profileType"></param>
        /// <returns></returns>
        static string GetProfileTypeString(IMAPI_PROFILE_TYPE profileType)
        {
            switch (profileType)
            {
                default:
                    return string.Empty;

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_RECORDABLE:
                    return "CD-R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_CD_REWRITABLE:
                    return "CD-RW";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVDROM:
                    return "DVD ROM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_RECORDABLE:
                    return "DVD-R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_RAM:
                    return "DVD-RAM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_R:
                    return "DVD+R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_RW:
                    return "DVD+RW";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_R_DUAL:
                    return "DVD+R Dual Layer";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_REWRITABLE:
                    return "DVD-RW";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_RW_SEQUENTIAL:
                    return "DVD-RW Sequential";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_SEQUENTIAL:
                    return "DVD-R DL Sequential";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_DASH_R_DUAL_LAYER_JUMP:
                    return "DVD-R Dual Layer";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_DVD_PLUS_RW_DUAL:
                    return "DVD+RW DL";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_HD_DVD_ROM:
                    return "HD DVD-ROM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_HD_DVD_RECORDABLE:
                    return "HD DVD-R";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_HD_DVD_RAM:
                    return "HD DVD-RAM";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_ROM:
                    return "Blu-ray DVD (BD-ROM)";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_R_SEQUENTIAL:
                    return "Blu-ray media Sequential";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_R_RANDOM_RECORDING:
                    return "Blu-ray media";

                case IMAPI_PROFILE_TYPE.IMAPI_PROFILE_TYPE_BD_REWRITABLE:
                    return "Blu-ray Rewritable media";
            }
        }

        ///// <summary>
        ///// Provides the display string for an IDiscRecorder2 object in the combobox
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void devicesComboBox_Format(object sender, ListControlConvertEventArgs e)
        //{
        //    IDiscRecorder2 discRecorder2 = (IDiscRecorder2)e.ListItem;
        //    string devicePaths = string.Empty;
        //    string volumePath = (string)discRecorder2.VolumePathNames.GetValue(0);
        //    foreach (string volPath in discRecorder2.VolumePathNames)
        //    {
        //        if (!string.IsNullOrEmpty(devicePaths))
        //        {
        //            devicePaths += ",";
        //        }
        //        devicePaths += volumePath;
        //    }

        //    e.Value = string.Format("{0} [{1}]", devicePaths, discRecorder2.ProductId);
        //}
        #endregion

        #region Media Size

        //private void ButtonDetectMediaClick(object sender, RoutedEventArgs e)
        //{
        //    if (devicesComboBox.SelectedIndex == -1)
        //    {
        //        return;
        //    }

        //    var discRecorder =
        //        (IDiscRecorder2)devicesComboBox.Items[devicesComboBox.SelectedIndex];

        //    MsftFileSystemImage fileSystemImage = null;
        //    MsftDiscFormat2Data discFormatData = null;

        //    try
        //    {
        //        //
        //        // Create and initialize the IDiscFormat2Data
        //        //
        //        discFormatData = new MsftDiscFormat2Data();
        //        if (!discFormatData.IsCurrentMediaSupported(discRecorder))
        //        {
        //            MediaType= "Media not supported";
        //            totalDiscSize = 0;
        //            return;
        //        }
        //        else
        //        {
        //            //
        //            // Get the media type in the recorder
        //            //
        //            discFormatData.Recorder = discRecorder;
        //            IMAPI_MEDIA_PHYSICAL_TYPE mediaType = discFormatData.CurrentPhysicalMediaType;
        //            MediaType= GetMediaTypeString(mediaType);

        //            //
        //            // Create a file system and select the media type
        //            //
        //            fileSystemImage = new MsftFileSystemImage();
        //            fileSystemImage.ChooseImageDefaultsForMediaType(mediaType);

        //            //
        //            // See if there are other recorded sessions on the disc
        //            //
        //            if (!discFormatData.MediaHeuristicallyBlank)
        //            {
        //                fileSystemImage.MultisessionInterfaces = discFormatData.MultisessionInterfaces;
        //                fileSystemImage.ImportFileSystem();
        //            }

        //            Int64 freeMediaBlocks = fileSystemImage.FreeMediaBlocks;
        //            totalDiscSize = 2048 * freeMediaBlocks;
        //        }
        //    }
        //    catch (COMException exception)
        //    {
        //        MessageBox.Show(exception.Message, "Detect Media Error",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        if (discFormatData != null)
        //        {
        //            Marshal.ReleaseComObject(discFormatData);
        //        }

        //        if (fileSystemImage != null)
        //        {
        //            Marshal.ReleaseComObject(fileSystemImage);
        //        }
        //    }


        //    UpdateCapacity();
        //}

        /// <summary>
        /// Updates the capacity MetroProgressBar
        /// </summary>
        private void UpdateCapacity()
        {
            //
            // Get the text for the Max Size
            //
            if (totalDiscSize == 0)
            {
                TotalSize = "0MB";
                return;
            }

            TotalSize = totalDiscSize < 1000000000 ?
                string.Format("{0}MB", totalDiscSize / 1000000) :
                string.Format("{0:F2}GB", totalDiscSize / 1000000000.0);

            //
            // Calculate the size of the files
            //
            Int64 totalMediaSize = MediaItems.Sum(mediaItem => mediaItem.SizeOnDisc);

            if (totalMediaSize == 0)
            {
                CapacityProgressValue = 0;
                CapacityProgressBrush = Brushes.Yellow;
            }
            else
            {
                var percent = (int)((totalMediaSize * 100) / totalDiscSize);
                if (percent > 100)
                {
                    CapacityProgressValue = 100;
                    CapacityProgressBrush = Brushes.Red;
                }
                else
                {
                    CapacityProgressValue = percent;
                    CapacityProgressBrush = Brushes.Yellow;
                }
            }
        }

        #endregion

        #region Burn Media Process


        /// <summary>
        /// The thread that does the burning of the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundBurnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            MsftDiscRecorder2 discRecorder = null;
            MsftDiscFormat2Data discFormatData = null;

            try
            {
                //
                // Create and initialize the IDiscRecorder2 object
                //
                discRecorder = new MsftDiscRecorder2();
                var data = (BurnData)e.Argument;
                discRecorder.InitializeDiscRecorder(data.uniqueRecorderId);

                //
                // Create and initialize the IDiscFormat2Data
                //
                discFormatData = new MsftDiscFormat2Data
                {
                    Recorder = discRecorder,
                    ClientName = ClientName,
                    ForceMediaToBeClosed = closeMedia
                };

                //
                // Set the verification level
                //
                var burnVerification = (IBurnVerification)discFormatData;
                burnVerification.BurnVerificationLevel = verificationLevel;

                //
                // Check if media is blank, (for RW media)
                //
                object[] multisessionInterfaces = null;
                if (!discFormatData.MediaHeuristicallyBlank)
                {
                    multisessionInterfaces = discFormatData.MultisessionInterfaces;
                }

                //
                // Create the file system
                //
                IStream fileSystem;
                if (!CreateMediaFileSystem(discRecorder, multisessionInterfaces, out fileSystem))
                {
                    e.Result = -1;
                    return;
                }

                //
                // add the Update event handler
                //
                discFormatData.Update += DiscFormatDataUpdate;

                //
                // Write the data here
                //
                try
                {
                    discFormatData.Write(fileSystem);
                    e.Result = 0;
                }
                catch (COMException ex)
                {
                    e.Result = ex.ErrorCode;

                    var msgBox = GetService<IMessageBoxService>();

                    if (msgBox != null)
                    {
                        msgBox.Show(ex.Message, "IDiscFormat2Data.Write failed", MessageBoxButton.OK,
                                    MessageBoxImage.Stop);
                    }

                }
                finally
                {
                    if (fileSystem != null)
                    {
                        Marshal.FinalReleaseComObject(fileSystem);
                    }
                }

                //
                // remove the Update event handler
                //
                discFormatData.Update -= DiscFormatDataUpdate;

                if (ejectMedia)
                {
                    discRecorder.EjectMedia();
                }
            }
            catch (COMException exception)
            {
   
                e.Result = exception.ErrorCode;

                var msgBox = GetService<IMessageBoxService>();

                if (msgBox != null)
                {
                    msgBox.Show(exception.Message, "IDiscFormat2Data.Write failed", MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                }
            }
            finally
            {
                if (discRecorder != null)
                {
                    Marshal.ReleaseComObject(discRecorder);
                }

                if (discFormatData != null)
                {
                    Marshal.ReleaseComObject(discFormatData);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progress"></param>
        void DiscFormatDataUpdate([In, MarshalAs(UnmanagedType.IDispatch)] object sender, [In, MarshalAs(UnmanagedType.IDispatch)] object progress)
        {
            //
            // Check if we've cancelled
            //
            if (backgroundBurnWorker.CancellationPending)
            {
                var format2Data = (IDiscFormat2Data)sender;
                format2Data.CancelWrite();
                return;
            }

            var eventArgs = (IDiscFormat2DataEventArgs)progress;

            burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING;

            // IDiscFormat2DataEventArgs Interface
            burnData.elapsedTime = eventArgs.ElapsedTime;
            burnData.remainingTime = eventArgs.RemainingTime;
            burnData.totalTime = eventArgs.TotalTime;

            // IWriteEngine2EventArgs Interface
            burnData.currentAction = eventArgs.CurrentAction;
            burnData.startLba = eventArgs.StartLba;
            burnData.sectorCount = eventArgs.SectorCount;
            burnData.lastReadLba = eventArgs.LastReadLba;
            burnData.lastWrittenLba = eventArgs.LastWrittenLba;
            burnData.totalSystemBuffer = eventArgs.TotalSystemBuffer;
            burnData.usedSystemBuffer = eventArgs.UsedSystemBuffer;
            burnData.freeSystemBuffer = eventArgs.FreeSystemBuffer;

            //
            // Report back to the UI
            //
            backgroundBurnWorker.ReportProgress(0, burnData);
        }

        /// <summary>
        /// Completed the "Burn" thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundBurnWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BurnStatusMsg = (int)e.Result == 0 ? "Finished burning disc" : "Error Burning Disc!";
            BurnProgressValue = 0;

            isBurning = false;
            EnableBurnUi(true);
            //buttonBurn.IsEnabled = true;
        }

        /// <summary>
        /// Enables/Disables the "Burn" User Interface
        /// </summary>
        /// <param name="enable"></param>
        void EnableBurnUi(bool enable)
        {
             
            //buttonBurn.Content = enable ? "Burn" : "Cancel";

            //buttonDetectMedia.IsEnabled = enable;

            //devicesComboBox.IsEnabled = enable;
            //listBoxFiles.IsEnabled = enable;

            //buttonAddFiles.IsEnabled = enable;
            //buttonAddFolders.IsEnabled = enable;
            //buttonRemoveFiles.IsEnabled = enable;
            //checkBoxEject.IsEnabled = enable;
            //checkBoxCloseMedia.IsEnabled = enable;
            //textBoxLabel.IsEnabled = enable;
            //comboBoxVerification.IsEnabled = enable;
        }

        /// <summary>
        /// Event receives notification from the Burn thread of an event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundBurnWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //int percent = e.ProgressPercentage;
            var data = (BurnData)e.UserState;

            if (data.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM)
            {
                BurnStatusMsg = data.statusMessage;
            }
            else if (data.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING)
            {
                switch (data.currentAction)
                {
                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VALIDATING_MEDIA:
                        BurnStatusMsg = "Validating current media...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FORMATTING_MEDIA:
                        BurnStatusMsg = "Formatting media...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_INITIALIZING_HARDWARE:
                        BurnStatusMsg = "Initialising hardware...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_CALIBRATING_POWER:
                        BurnStatusMsg = "Optimising laser intensity...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_WRITING_DATA:
                        long writtenSectors = data.lastWrittenLba - data.startLba;

                        if (writtenSectors > 0 && data.sectorCount > 0)
                        {
                            var percent = (int)((100 * writtenSectors) / data.sectorCount);
                            BurnStatusMsg = string.Format("Progress: {0}%", percent);
                            BurnProgressValue = percent;
                        }
                        else
                        {
                            BurnStatusMsg = "Progress 0%";
                            BurnProgressValue = 0;
                        }
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FINALIZATION:
                        BurnStatusMsg = "Finalising writing...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_COMPLETED:
                        BurnStatusMsg = "Completed!";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VERIFYING:
                        BurnStatusMsg = "Verifying";
                        break;
                }
            }
        }
 

        #endregion

        #region File System Process
       
        private bool CreateMediaFileSystem(IDiscRecorder2 discRecorder, object[] multisessionInterfaces, out IStream dataStream)
        {
            MsftFileSystemImage fileSystemImage = null;
            try
            {
                fileSystemImage = new MsftFileSystemImage();
                fileSystemImage.ChooseImageDefaults(discRecorder);
                fileSystemImage.FileSystemsToCreate =
                    FsiFileSystems.FsiFileSystemJoliet | FsiFileSystems.FsiFileSystemISO9660;


                //if (!textBoxLabel.Dispatcher.CheckAccess())
                //{
                //    textBoxLabel.Dispatcher.Invoke(
                //      System.Windows.Threading.DispatcherPriority.Normal, new Action(() => fileSystemImage.VolumeName = textBoxLabel.Text));
                //}
                //else
                //{
                //    fileSystemImage.VolumeName = textBoxLabel.Text;
                //}



                fileSystemImage.Update += FileSystemImageUpdate;

                //
                // If multisessions, then import previous sessions
                //
                if (multisessionInterfaces != null)
                {
                    fileSystemImage.MultisessionInterfaces = multisessionInterfaces;
                    fileSystemImage.ImportFileSystem();
                }

                //
                // Get the image root
                //
                IFsiDirectoryItem rootItem = fileSystemImage.Root;

                //
                // Add Files and Directories to File System Image
                //

  
                foreach (IMediaItem mediaItem in MediaItems)
                {
                    //
                    // Check if we've cancelled
                    //
                    if (backgroundBurnWorker.CancellationPending)
                    {
                        break;
                    }

                    //
                    // Add to File System
                    //
                    mediaItem.AddToFileSystem(rootItem);
                }

                fileSystemImage.Update -= FileSystemImageUpdate;

                //
                // did we cancel?
                //
                if (backgroundBurnWorker.CancellationPending)
                {
                    dataStream = null;
                    return false;
                }

                dataStream = fileSystemImage.CreateResultImage().ImageStream;
            }
            catch (COMException exception)
            {

                var msgBox = GetService<IMessageBoxService>();

                if (msgBox != null)
                {
                    msgBox.Show(exception.Message, "Create file system error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                }
                dataStream = null;
                return false;
            }
            finally
            {
                if (fileSystemImage != null)
                {
                    Marshal.ReleaseComObject(fileSystemImage);
                }
            }

            return true;
        }

        /// <summary>
        /// Event Handler for File System Progress Updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="currentFile"></param>
        /// <param name="copiedSectors"></param>
        /// <param name="totalSectors"></param>
        void FileSystemImageUpdate([In, MarshalAs(UnmanagedType.IDispatch)] object sender,
            [In, MarshalAs(UnmanagedType.BStr)]string currentFile, [In] int copiedSectors, [In] int totalSectors)
        {
            var percentProgress = 0;
            if (copiedSectors > 0 && totalSectors > 0)
            {
                percentProgress = (copiedSectors * 100) / totalSectors;
            }

            if (!string.IsNullOrEmpty(currentFile))
            {
                var fileInfo = new FileInfo(currentFile);
                burnData.statusMessage = "Adding \"" + fileInfo.Name + "\" to image...";

                //
                // report back to the ui
                //
                burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM;
                backgroundBurnWorker.ReportProgress(percentProgress, burnData);
            }

        }
        #endregion

        #region Format/Erase the Disc

        /// <summary>
        /// Enables/Disables the "Burn" User Interface
        /// </summary>
        /// <param name="enable"></param>
        void EnableFormatUi(bool enable)
        {
            //buttonFormat.IsEnabled = enable;
            //checkBoxEjectFormat.IsEnabled = enable;
            //checkBoxQuickFormat.IsEnabled = enable;
        }

        /// <summary>
        /// Worker thread that Formats the Disc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundFormatWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            MsftDiscRecorder2 discRecorder = null;
            MsftDiscFormat2Erase discFormatErase = null;

            try
            {
                //
                // Create and initialize the IDiscRecorder2
                //
                discRecorder = new MsftDiscRecorder2();
                var activeDiscRecorder = (string)e.Argument;
                discRecorder.InitializeDiscRecorder(activeDiscRecorder);

                //
                // Create the IDiscFormat2Erase and set properties
                //

                if (ShouldFormatQuick)
                    discFormatErase = new MsftDiscFormat2Erase
                                          {
                                              Recorder = discRecorder,
                                              ClientName = ClientName,
                                              FullErase = !ShouldFormatQuick
                                          };

                //
                // Setup the Update progress event handler
                //
                if (discFormatErase != null)
                {
                    discFormatErase.Update += DiscFormatEraseUpdate;

                    //
                    // Erase the media here
                    //
                    try
                    {
                        discFormatErase.EraseMedia();
                        e.Result = 0;
                    }
                    catch (COMException ex)
                    {
                        e.Result = ex.ErrorCode;
                        var msgBox = GetService<IMessageBoxService>();
                        if (msgBox != null)
                        {
                            msgBox.Show(ex.Message, "IDiscFormat2.EraseMedia failedr", MessageBoxButton.OK,
                                        MessageBoxImage.Stop);
                        }
      
                    }

                    //
                    // Remove the Update progress event handler
                    //
                    discFormatErase.Update -= DiscFormatEraseUpdate;
                }

                //
                // Eject the media 
                //

                if (ShouldEjectFormat)
                {
                    discRecorder.EjectMedia();
                }
            }
            catch (COMException exception)
            {
                //
                // If anything happens during the format, show the message
                //
                var msgBox = GetService<IMessageBoxService>();
                if (msgBox != null)
                {
                    msgBox.Show(exception.Message, exception.Source, MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                }
      
            }
            finally
            {
                if (discRecorder != null)
                {
                    Marshal.ReleaseComObject(discRecorder);
                }

                if (discFormatErase != null)
                {
                    Marshal.ReleaseComObject(discFormatErase);
                }
            }
        }

        /// <summary>
        /// Event Handler for the Erase Progress Updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedSeconds"></param>
        /// <param name="estimatedTotalSeconds"></param>
        void DiscFormatEraseUpdate([In, MarshalAs(UnmanagedType.IDispatch)] object sender, int elapsedSeconds, int estimatedTotalSeconds)
        {
            var percent = elapsedSeconds * 100 / estimatedTotalSeconds;
            //
            // Report back to the UI
            //
            backgroundFormatWorker.ReportProgress(percent);
        }

        private void BackgroundFormatWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FormatStatusMsg= string.Format("Formatting {0}%...", e.ProgressPercentage);
            FormatProgressValue = e.ProgressPercentage;
        }

        private void BackgroundFormatWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FormatStatusMsg= (int)e.Result == 0 ?
                "Finished Formatting Disc!" : "Error Formatting Disc!";

            FormatProgressValue = 0;

            isFormatting = false;
            EnableFormatUi(true);
        }
        #endregion

        ///// <summary>
        ///// Called when user selects a new tab
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        //{
        //    //
        //    // Prevent page from changing if we're burning or formatting.
        //    //
        //    if (_isBurning || _isFormatting)
        //    {
        //        e.Cancel = true;
        //    }
        //}

        ///// <summary>
        ///// Get the burn verification level when the user changes the selection
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ComboBoxVerificationSelectionChanged(object sender, RoutedEventArgs e)
        //{
        //    verificationLevel = (IMAPI_BURN_VERIFICATION_LEVEL)comboBoxVerification.SelectedIndex;
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var discRecorder = (IDiscRecorder2)devicesComboBox.Items[devicesComboBox.SelectedIndex];
        //        discRecorder.EjectMedia();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
        //    }


        //}
    }
}

