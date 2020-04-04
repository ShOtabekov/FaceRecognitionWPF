using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using FaceDetectionAndRecognition.Database;
using FaceDetectionAndRecognition.Model;
using FaceDetectionAndRecognition.ViewModels;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FaceDetectionAndRecognition
{

    public partial class WFFaceRecognition : MetroWindow, INotifyPropertyChanged
    {
        EntityContext entityContext;
        IEnumerable<User> users;
        List<FoundedUserViewModel> foundedUserViews;


        public WFFaceRecognition()
        {
            InitializeComponent();
            captureTimer = new Timer()
            {
                Interval = Config.TimerResponseValue

            };
            captureTimer.Elapsed += CaptureTimer_Elapsed;
            DataContext = this;
            entityContext = EntityContext.CreateInstance();
            users = entityContext.Users.ToList();
            foundedUserViews = new List<FoundedUserViewModel>();

            this.RegistrationView = new RegistrationViewModel();
        }

        #region Fields
        public event PropertyChangedEventHandler PropertyChanged;
        Capture videoCapture;
        HaarCascade haarCascade;
        Image<Bgr, Byte> bgrFrame = null;
        Image<Gray, Byte> detectedFace = null;
        List<FaceData> faceList = new List<FaceData>();
        List<Image<Gray, Byte>> imageList = new List<Image<Gray, byte>>();
        List<string> lList = new List<string>();
        Timer captureTimer;
        #endregion

        #region Properties



        string faceName;
        public string FaceName
        {
            get { return faceName; }
            set
            {
                faceName = value.ToUpper();
                //lblFaceName.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lblFaceName.Text = faceName; }));
                NotifyPropertyChanged();
            }
        }

        Bitmap cameraCapture;
        public Bitmap CameraCapture
        {
            get { return cameraCapture; }
            set
            {
                cameraCapture = value;
                imgCamera.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { imgCamera.Source = BitmapToImageSource(cameraCapture); }));
                NotifyPropertyChanged();
            }
        }

        Bitmap cameraCaptureFace;
        public Bitmap CameraCaptureFace
        {
            get { return cameraCaptureFace; }
            set
            {
                cameraCaptureFace = value;
                //imgDetectFace.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { imgDetectFace.Source = BitmapToImageSource(cameraCaptureFace); }));
                // imgCamera.Source = BitmapToImageSource(cameraCapture);
                NotifyPropertyChanged();
            }
        }

        public RegistrationViewModel RegistrationView { get; set; }
        FoundedUserViewModel foundedUserViewModel;
        public FoundedUserViewModel FoundedUserViewModel
        {
            get => foundedUserViewModel;
            set { foundedUserViewModel = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region Event
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetFacesList();
            videoCapture = new Capture(Config.ActiveCameraIndex);
            videoCapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS, 30);
            videoCapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 450);
            videoCapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 370);
            captureTimer.Start();
        }
        void CaptureTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ProcessFrame();
        }
        void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            WFAbout wfAbout = new WFAbout();
            wfAbout.ShowDialog();
        }
        void NewFaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (detectedFace == null)
            {
                MessageBox.Show("Қиёфа ёфт нашуд.");
                return;
            }
            //Save detected face
            detectedFace = detectedFace.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            detectedFace.Save(Config.FacePhotosPath + "face" + (faceList.Count + 1) + Config.ImageFileExtension);
            StreamWriter writer = new StreamWriter(Config.FaceListTextFile, true);
            string personName = Microsoft.VisualBasic.Interaction.InputBox("Номро дохил намоед");
            writer.WriteLine(String.Format("face{0}:{1}", (faceList.Count + 1), personName));
            writer.Close();
            GetFacesList();
            MessageBox.Show("Бомуваффикият сабт шуд .");
        }
        void OpenVideoFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog().Value == true)
            {
                captureTimer.Stop();
                videoCapture.Dispose();

                videoCapture = new Capture(openDialog.FileName);
                captureTimer.Start();
                this.Title = openDialog.FileName;
                return;
            }



        }
        #endregion

        #region Method
        public void GetFacesList()
        {
            var currentPath = Directory.GetCurrentDirectory();
            var haarcascadePath = Path.Combine(currentPath, "Lib", "haarcascade.xml");
            //haar cascade classifier
            haarCascade = new HaarCascade(haarcascadePath);
            faceList.Clear();
            string line;
            FaceData faceInstance = null;
            //split face text file
            StreamReader reader = new StreamReader(Config.FaceListTextFile);
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineParts = line.Split(':');
                faceInstance = new FaceData();
                faceInstance.FaceImage = new Image<Gray, byte>(Config.FacePhotosPath + lineParts[0] + Config.ImageFileExtension);
                faceInstance.PersonName = lineParts[1];
                faceList.Add(faceInstance);
            }
            foreach (var face in faceList)
            {
                imageList.Add(face.FaceImage);
                lList.Add(face.PersonName);
            }
            reader.Close();
        }

        private void ProcessFrame()
        {
            bgrFrame = videoCapture.QueryFrame();

            if (bgrFrame != null)
            {
                try
                {//for emgu cv bug
                    Image<Gray, byte> grayframe = bgrFrame.Convert<Gray, byte>();
                    //detect face
                    MCvAvgComp[][] faces = grayframe.DetectHaarCascade(haarCascade, 1.2, 10, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));

                    FaceName = "Қиёфа ётф нашуд!!!!";
foreach (var face in faces[0])
                    {
                        bgrFrame.Draw(face.rect, new Bgr(255, 255, 0), 2);
                        detectedFace = bgrFrame.Copy(face.rect).Convert<Gray, byte>();
                        FaceRecognition();
                        break;
                    }                    CameraCapture = bgrFrame.ToBitmap();
                }
                catch (Exception ex)
                {

                    //todo log
                }

            }
        }

        private void FaceRecognition()
        {
            if (imageList.ToArray().Length != 0)
            {
                MCvTermCriteria termCrit = new MCvTermCriteria(lList.Count, 0.001);
                //Eigen Face Algorithm
                EigenObjectRecognizer recognizer = new EigenObjectRecognizer(imageList.ToArray(), lList.ToArray(), 3000, ref termCrit);
                string faceName = recognizer.Recognize(detectedFace.Resize(100, 100, INTER.CV_INTER_CUBIC));
                FaceName = faceName;

                if (foundedUserViews.FirstOrDefault(u => u.FullName == faceName) is FoundedUserViewModel foundedUser && foundedUser != null)
                {
                    FoundedUserViewModel = foundedUser;
                }
                else if (users.FirstOrDefault(u => u.FullName == faceName) is User user && user != null)
                {
                    FoundedUserViewModel = new FoundedUserViewModel(user.FirstName, user.LastName, user.Birthday, user.PhoneNumber, user.Address);
                    foundedUserViews.Add(FoundedUserViewModel);
                }

                CameraCaptureFace = detectedFace.ToBitmap();
            }
            else
            {
                FaceName = "Қиёфаро дохил намоед!!!!";
            }
        }
        /// <summary>
        /// Convert bitmap to bitmap image for image control
        /// </summary>
        /// <param name="bitmap">Bitmap image</param>
        /// <returns>Image Source</returns>
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        #endregion

        private void uiSavePerson_Click(object sender, RoutedEventArgs e)
        {
            if (detectedFace == null)
            {
                MessageBox.Show("Қиёфа ёфт нашуд.");
                return;
            }
            var personName = $"{RegistrationView.FirstName} {RegistrationView.LastName}";
            if (string.IsNullOrEmpty(personName.Trim()) || string.IsNullOrEmpty(RegistrationView.FirstName) || string.IsNullOrEmpty(RegistrationView.LastName))
            {
                MessageBox.Show("Пеш аз сабт кар шумо бояд ном ва насабро дароред");
            }
            //Save detected face
            detectedFace = detectedFace.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            detectedFace.Save(Config.FacePhotosPath + "face" + (faceList.Count + 1) + Config.ImageFileExtension);
            StreamWriter writer = new StreamWriter(Config.FaceListTextFile, true);
            writer.WriteLine(String.Format("face{0}:{1}", (faceList.Count + 1), personName));
            if (!entityContext.Users.Any(u => u.LastName == RegistrationView.LastName && u.FirstName == RegistrationView.FirstName))
            {
                var user = new User()
                {
                    FirstName = RegistrationView.FirstName,
                    LastName = RegistrationView.LastName,
                    Birthday = RegistrationView.Birthday,
                    Address = RegistrationView.Address,
                    PhoneNumber = RegistrationView.PhoneNumber
                };
                entityContext.Users.Add(user);
                entityContext.SaveChanges();
            }

            writer.Close();
            GetFacesList();
            MessageBox.Show("Бомуваффикият сабт шуд .");
        }
    }
}
