using AForge.Video.DirectShow;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WPF_MachineService.Models;
using WPF_MachineService.Repository;
using WPF_MachineService.Service;
namespace WPF_MachineService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ScanMachineContext context;
        public UnitOfWork unitOfWork = new UnitOfWork();
        private FilterInfoCollection? _filterInfoCollectionVideoDevices;
        private VideoCaptureDevice[]? _videoCaptureDeviceSources;
        private int selectedCameraIndex = 0;
        private int captureCount = 1;
        private bool IsScanning = true;
        public MainWindow()
        {
            context = new ScanMachineContext();
            InitializeComponent();
            Loaded += MachineWindow_Loaded;
            Closing += MachineWindow_Closing;

        }

        /// <summary>
        /// Load Main Window, Sceen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MachineWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            _filterInfoCollectionVideoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (_filterInfoCollectionVideoDevices != null && _filterInfoCollectionVideoDevices.Count >= 0)
            {
                _videoCaptureDeviceSources = new VideoCaptureDevice[1];
                _videoCaptureDeviceSources[0] = new VideoCaptureDevice(_filterInfoCollectionVideoDevices[0].MonikerString);
                _videoCaptureDeviceSources[0].NewFrame += VideoSource_BitMapFrame;
                _videoCaptureDeviceSources[0].Start();
            }
            else
            {
                MessageBox.Show("Không đủ thiết bị video.");
            }

        }
        /// <summary>
        /// Use Bitmap in Video && Image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void VideoSource_BitMapFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap videoCapture = (Bitmap)eventArgs.Frame.Clone();
                imgVideo.Dispatcher.Invoke(() => DisplayImageInImageView(videoCapture, imgVideo));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in VideoSource_BitMapFrame: {ex.Message}");
            }
        }

        /// <summary>
        /// Conver , dispay image
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="imageView"></param>
        private void DisplayImageInImageView(Bitmap frame, System.Windows.Controls.Image imageView)
        {
            System.Windows.Media.Imaging.BitmapImage bitmapImage = ToBitMapImage(frame);

            if (bitmapImage != null)
            {
                imageView.Source = bitmapImage;
            }
        }
        private System.Windows.Media.Imaging.BitmapImage ToBitMapImage(Bitmap bitmap)
        {
            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        /// <summary>
        /// Closing window 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MachineWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_videoCaptureDeviceSources != null)
                {
                    foreach (var videoSource in _videoCaptureDeviceSources)
                    {
                        if (videoSource != null && videoSource.IsRunning)
                        {
                            await Task.Run(() =>
                            {
                                videoSource.SignalToStop();
                                videoSource.WaitForStop();
                            });
                        }
                    }
                    _videoCaptureDeviceSources = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi dừng video source: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Bitmap HelpToBitMapImage(BitmapSource capturedBitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(capturedBitmapSource));
                enc.Save(memoryStream);
                bitmap = new Bitmap(memoryStream);
            }
            return bitmap;
        }
        private void btTakePictureImage_Click(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                if (_videoCaptureDeviceSources != null)
                {
                    if (selectedCameraIndex >= 0)
                    {
                        var videoSource = _videoCaptureDeviceSources[selectedCameraIndex];
                        if (videoSource != null && videoSource.IsRunning)
                        {
                            BitmapSource capturedBitmapSource = (BitmapSource)imgVideo.Source;
                            if (capturedBitmapSource != null)
                            {
                                Bitmap capturedBitmap = HelpToBitMapImage(capturedBitmapSource);
                                string subfolderName = DateTime.Now.ToString("yyyyMMdd");
                                string subfolderPath = System.IO.Path.Combine("D:\\Dev\\BE\\WPF\\Wpf_projects\\SavePic", subfolderName);

                                if (!Directory.Exists(subfolderPath))
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(subfolderPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Windows.MessageBox.Show($"Error creating subfolder: {ex.Message}");
                                        return;
                                    }
                                }
                                string folderName = $"{DateTime.Now:yyyyMMdd}_{captureCount++}";
                                string folderPath = System.IO.Path.Combine(subfolderPath, folderName);
                                if (!Directory.Exists(folderPath))
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(folderPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Windows.MessageBox.Show($"Error creating folder: {ex.Message}");
                                        return;
                                    }
                                }
                                string fileName = $"capture_{DateTime.Now:HHmmss}.png";

                                string filePath = System.IO.Path.Combine(folderPath, fileName);
                                string filePathPython = System.IO.Path.Combine("C:\\ultralytics\\yolov8-silva\\inference\\images", fileName);
                                LoadDetectionData();
                                try
                                {
                                    capturedBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                                    capturedBitmap.Save(filePathPython, System.Drawing.Imaging.ImageFormat.Bmp);
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.MessageBox.Show($"Error saving capture: {ex.Message}");
                                }
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Bitmap source is null.");
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Selected video source is not running.");
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Invalid camera index.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error capturing frame: {ex.Message}");
            }

        }

        private async void LoadDetectionData()
        {
            string detectjsonFilePath = @"D:\Materials\SWD\Code\ScanMachine_SWD\WPF_MachineService\WPF_MachineService\WPF_MachineService\Detection_results.json";
            Scanning messageBox = new Scanning();
            Window window = new Window
            {
                Content = messageBox,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "Scanning"
            };
            window.Show();
            await Task.Delay(2000);
            if (File.Exists(detectjsonFilePath))
            {
                List<Detection> productsFromJson;
                using (StreamReader r = new StreamReader(detectjsonFilePath))
                {
                    string json = r.ReadToEnd();
                    productsFromJson = JsonConvert.DeserializeObject<List<Detection>>(json);
                }
                Dictionary<string, int> productNameCounts = new Dictionary<string, int>();
                foreach (var detection in productsFromJson)
                {
                    if (productNameCounts.ContainsKey(detection.name))
                    {
                        productNameCounts[detection.name]++;
                    }
                    else
                    {
                        productNameCounts[detection.name] = 1;
                    }
                }
                List<Product> productsToDisplay = new List<Product>();
                var allProductsFromDb = context.Products.ToList();
                foreach (var productDb in allProductsFromDb)
                {
                    if (productNameCounts.ContainsKey(productDb.ProductName))
                    {
                        productDb.Quantity = productNameCounts[productDb.ProductName];
                        productDb.Price = productDb.Quantity * productDb.Price;
                        productsToDisplay.Add(productDb);
                    };
                }

                lvListView.ItemsSource = productsToDisplay;
                lvListView.DataContext = this;
                CalculateTotalPrice();
                IsScanning = false;
            }
            else
            {
                MessageBox.Show("File not found: " + detectjsonFilePath);
            }
            window.Close();
        }
        private void CalculateTotalPrice()
        {
            double totalPrice = 0;
            foreach (var item in lvListView.Items)
            {
                if (item is Product product)
                {
                    totalPrice += product.Quantity * product.Price;
                }
            }
            tbSumTotal.Text = totalPrice.ToString();
        }

        private void ResultTotolPrice(object sender, TextChangedEventArgs e)
        {
            CalculateTotalPrice();
        }
        private async void btPayment(object sender, RoutedEventArgs e)
        {
            string Phone = "0708125538";
            string Name = "Nguyễn Gia Đạt";
            string Email = "dat36226@gmail.com";
            string PayNumber = tbSumTotal.Text.Trim();
            string Datetimes = DateTime.Now.ToString("dd/MM/yyyy");

            if (lvListView.ItemsSource is IEnumerable<Product> products)
            {
                foreach (var selectedProduct in products)
                {
                    string productName = selectedProduct.ProductName;
                    string Description = $"Sản phẩm {productName} + Giá tiền {PayNumber} + {Datetimes}";
                    MomoQRCodeGenerator momoGenerator = new MomoQRCodeGenerator();
                    string merchantCode = $"2|99|{Phone}|{Name}|{Email}|0|0|{PayNumber}|{Description}";
                    Bitmap momoQRCode = momoGenerator.GenerateMomoQRCode(merchantCode);
                    Bitmap resizedLogo = ResizeImage(Properties.Resources.logo, 50, 50);
                    momoQRCode = AddLogoToQRCode(momoQRCode, resizedLogo);


                    if (!string.IsNullOrWhiteSpace(tbSumTotal.Text))
                    {
                        ScanMoMoQR scanQR = new ScanMoMoQR();
                        scanQR.UpdateQRCode(momoQRCode);
                        double windowWidth = 500;
                        double windowHeight = 500;
                        Window qrCodeWindow = new Window
                        {
                            Content = scanQR,
                            Width = windowWidth,
                            Height = windowHeight,
                            WindowStyle = WindowStyle.None,
                            ResizeMode = ResizeMode.NoResize,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            Title = "ScanQR"
                        };
                        qrCodeWindow.Show();
                        await Task.Delay(60000);        // Check time 
                        if (!ProcessPayment())
                        {
                            qrCodeWindow.Close();
                            MessageBox.Show("QR code quá thời gian. Vui lòng thanh toán lại !!!", "Error", MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid total price.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }
        private Bitmap ResizeImage(Bitmap image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(newImage);
            g.DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }
        private Bitmap AddLogoToQRCode(Bitmap qrCode, Bitmap logo)
        {
            int xPos = (qrCode.Width - logo.Width) / 2;
            int yPos = (qrCode.Height - logo.Height) / 2;
            using (Graphics g = Graphics.FromImage(qrCode))
            {
                g.DrawImage(logo, new System.Drawing.Point(xPos, yPos));
            }
            return qrCode;
        }
        private bool ProcessPayment()
        {
            bool paymentSuccess = false;


            return paymentSuccess;
        }
        private void btConfirmOrder(object sender, RoutedEventArgs e)
        {
            try
            {
                Models.Order newOrder = new Models.Order
                {
                    PaymentId = 1,
                    Total = Convert.ToDouble(tbSumTotal.Text),
                    Quantity = 1,
                    DateCreated = DateTime.Now,

                };

                unitOfWork.OrderRepository.Insert(newOrder);
                //unitOfWork.Save();

                if (lvListView.ItemsSource != null)
                {
                    foreach (var item in lvListView.ItemsSource)
                    {
                        if (item is Models.Product product)
                        {
                            // Kiểm tra xem product.Id có tồn tại trong bảng Product không
                            var existingProduct = unitOfWork.ProductRepository.Get(p => p.ProductId == product.ProductId);
                            if (existingProduct != null)
                            {
                                Models.OrderDetail orderDetail = new Models.OrderDetail
                                {
                                    OrderId = newOrder.OrderId,

                                    ProductId = product.ProductId,
                                    Price = product.Price,
                                    Quantity = product.Quantity,
                                    Status = "1",
                                };

                                newOrder.OrderDetails.Add(orderDetail);
                                newOrder.Total += product.Quantity * product.Price;
                            }
                            else
                            {
                                // Xử lý trường hợp product.Id không tồn tại trong bảng Product
                                MessageBox.Show($"Sản phẩm với Id {product.ProductId} không tồn tại trong cơ sở dữ liệu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return; // Dừng quá trình lưu đơn hàng
                            }
                        }
                    }
                }

                unitOfWork.Save();

                MessageBox.Show("Đơn hàng đã được lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}