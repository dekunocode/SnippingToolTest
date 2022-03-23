using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace SnappingToolTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _position;
        private bool _trimEnable = false;
        public System.Drawing.Bitmap Bitmap { get; }

        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Formロード時にサブディスプレイも含めた画面サイズを取得し
        /// それをWindowサイズにする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // プライマリスクリーンサイズの取得
            var screen = System.Windows.Forms.Screen.PrimaryScreen;


            // ウィンドウサイズの設定
            this.Left = screen.Bounds.Left;
            this.Top = screen.Bounds.Top;
            this.Height = screen.Bounds.Height;

            //すべてのディスプレイを列挙する
            //全ディスプレイが含まれる枠のサイズを作る
            foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
            {

                //全ディスプレイの一番左端の座標を取る
                if (this.Left > s.Bounds.Left)
                {
                    this.Left = s.Bounds.Left;
                }
                //全ディスプレイの一番上の座標を取る
                if (this.Top > s.Bounds.Top)
                {
                    this.Top = s.Bounds.Top;
                }
                //全ディスプレイの一番高い値を取得する
                if (this.Height < s.Bounds.Height)
                {
                    this.Height = s.Bounds.Height;
                }
                //全ディスプレイの幅を取得する
                this.Width += s.Bounds.Width;
            }

            // ジオメトリサイズの設定
            this.ScreenArea.Geometry1 = new RectangleGeometry(new Rect(this.Left, this.Top, this.Width, this.Height));
        }

        /// <summary>
        /// 画面を左クリックした座標を取得しキャプチャを開始する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawingPath_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var path = sender as Path;
            if (path == null)
                return;

            // 開始座標を取得
            var point = e.GetPosition(path);
            _position = point;

            // マウスキャプチャの設定
            _trimEnable = true;
            this.Cursor = Cursors.Cross;
            path.CaptureMouse();
        }

        /// <summary>
        /// 左クリックを離したらそこでキャプチャを終了して画像を取得する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawingPath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var path = sender as Path;
            if (path == null)
                return;

            // 現在座標を取得
            var point = e.GetPosition(path);

            // マウスキャプチャの終了
            _trimEnable = false;
            this.Cursor = Cursors.Arrow;
            path.ReleaseMouseCapture();

            // 画面キャプチャ
            CaptureScreen(point);

            // アプリケーションの終了
            this.Close();
        }

        /// <summary>
        /// マウスを動かしている時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawingPath_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_trimEnable)
                return;

            var path = sender as Path;
            if (path == null)
                return;

            // 現在座標を取得
            var point = e.GetPosition(path);

            // キャプチャ領域枠の描画
            DrawStroke(point);
        }

        /// <summary>
        /// 矩形の描画
        /// </summary>
        /// <param name="point"></param>
        private void DrawStroke(Point point)
        {
            var x = _position.X < point.X ? _position.X : point.X;
            var y = _position.Y < point.Y ? _position.Y : point.Y;
            var width = Math.Abs(point.X - _position.X);
            var height = Math.Abs(point.Y - _position.Y);
            this.ScreenArea.Geometry2 = new RectangleGeometry(new Rect(x, y, width, height));
        }

        /// <summary>
        /// 画面キャプチャ―の実施
        /// </summary>
        /// <param name="point"></param>
        private void CaptureScreen(Point point)
        {
            // 座標変換
            var start = PointToScreen(_position);
            var end = PointToScreen(point);

            // キャプチャエリアの取得
            var x = start.X < end.X ? (int)start.X : (int)end.X;
            var y = start.Y < end.Y ? (int)start.Y : (int)end.Y;
            var width = (int)Math.Abs(end.X - start.X);
            var height = (int)Math.Abs(end.Y - start.Y);
            if (width == 0 || height == 0)
            {
                return;
            }


            // スクリーンイメージの取得
            using (var bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            using (var graph = System.Drawing.Graphics.FromImage(bmp))
            {
                // 画面をコピーする
                graph.CopyFromScreen(new System.Drawing.Point(x, y), new System.Drawing.Point(), bmp.Size);

                // イメージの保存
                string folder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                bmp.Save(System.IO.Path.ChangeExtension(System.IO.Path.Combine(folder, "image"), "png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }

}
