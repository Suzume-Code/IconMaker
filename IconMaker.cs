using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Input;


namespace IconMaker {

	class Program {

		[STAThread]
		/// <summary>
		/// プログラムメイン
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args) {

			if (args.Length == 0)
				return;

			Converter converter = Converter.GetInstance();

			foreach (string filename in args) {
				if (File.Exists(filename)) {
					string outfilename = filename + ".ico";
					string extension = Path.GetExtension(filename).ToLower();
					if (extension.Equals(".png") || extension.Equals(".jpg") || extension.Equals(".bmp")) {
						using (Bitmap bmp = new Bitmap(filename))
							converter.ConvertToIcon(bmp, outfilename);
					}
				}
			}
		}
	}

	/// <summary>
	/// 画像コンバート
	/// </summary>
	class Converter {

        // インスタンス
        private static Converter singleton = null;

        public static Converter GetInstance() {

            if (singleton == null) {
                singleton = new Converter();
            }
            return singleton;
        }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		private Converter() {
		}

		/// <summary>
		/// アイコン形式へコンバート
		/// </summary>
		/// <param name="bmp"></param>
		/// <param name="outfilename"></param>
		/// <returns>作成したファイルの存在有無を返却</returns>
		public bool ConvertToIcon(Bitmap bmp, string outfilename) {

			KeyEvent keyevent = new KeyEvent();
			bool isTransparent = keyevent.IsKeyDown(Keys.LShiftKey);
			bool isResize = true;

			Bitmap resize_bmp = Resize(bmp, isResize, isTransparent);

			using (MemoryStream bitMapStream = new MemoryStream()) {
				 resize_bmp.Save(bitMapStream, ImageFormat.Png);
				 using (MemoryStream iconStream = new MemoryStream()) {
					using (BinaryWriter iconWriter = new BinaryWriter(iconStream)) {
						iconWriter.Write((short) 0);
						iconWriter.Write((short) 1);
						iconWriter.Write((short) 1);
						iconWriter.Write((byte) resize_bmp.Width);
						iconWriter.Write((byte) resize_bmp.Height);
						iconWriter.Write((short) 0);
						iconWriter.Write((short) 0);
						iconWriter.Write((short) 32);
						iconWriter.Write((int) bitMapStream.Length);
						iconWriter.Write(22);
						iconWriter.Write(bitMapStream.ToArray());
						iconWriter.Flush();
						iconWriter.Seek(0, SeekOrigin.Begin);
						using (Stream iconFileStream = new FileStream(outfilename, FileMode.Create)) {
							Icon icon = new Icon(iconStream);
							icon.Save(iconFileStream);
						}
					}
				 }
			}
			return File.Exists(outfilename);
		}

		/// <summary>
		/// ビットマップのサイズ調整
		/// </summary>
		/// <param name="bmp"></param>
		/// <param name="isResize"></param>
		/// <param name="isTransparent"></param>
		/// <returns>リサイズ後のビットマップ</returns>
		private Bitmap Resize(Bitmap bmp, bool isResize, bool isTransparent) {

			const int base_size = 128;

			int bmp_width = bmp.Width;
			int bmp_height = bmp.Height;
			int bmp_longer = (bmp_width >= bmp_height) ? bmp_width : bmp_height;

			decimal bmp_width_factor = (decimal) bmp_width / bmp_longer;
			decimal bmp_height_factor = (decimal) bmp_height / bmp_longer;

			decimal bmp_new_width = (bmp_width * bmp_width_factor);
			decimal bmp_new_height = (bmp_height * bmp_height_factor);

			if (isResize && bmp_longer > base_size) {
				bmp_longer = base_size;
				bmp_new_width = (base_size * bmp_width_factor);
				bmp_new_height = (base_size * bmp_height_factor);
			}
			decimal bmp_top_margin = (bmp_longer - bmp_new_height) / 2;
			decimal bmp_left_margin = (bmp_longer - bmp_new_width) / 2; 
			 

			Bitmap b = new Bitmap(bmp_longer, bmp_longer, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage((Image) b)) {
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(bmp, (float) bmp_left_margin, (float) bmp_top_margin, (float) bmp_new_width, (float) bmp_new_height);
			}
			if (isTransparent)
				b.MakeTransparent(b.GetPixel(0, 0));
			return b;
		}

	}

	/// <summary>
	/// User32クラス
	/// </summary>
	class User32 {

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern short GetKeyState(int keyCode);
	}

	/// <summary>
	/// キーイベントクラス
	/// </summary>
	class KeyEvent {
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public KeyEvent() {
		}

		/// <summary>
		/// キー状態の取得
		/// </summary>
		/// <param name="key"></param>
		/// <returns>キーの状態を返却</returns>
		private KeyStates GetKeyState(Keys key) {

			KeyStates state = KeyStates.None;

			short retVal = User32.GetKeyState((int)key);
			if ((retVal & 0x8000) == 0x8000)
				state |= KeyStates.Down;

			if ((retVal & 1) == 1)
				state |= KeyStates.Toggled;

			return state;
		}

		/// <summary>
		/// 指定キーのキーダウン判定
		/// </summary>
		/// <param name="key"></param>
		/// <returns>指定キーが押されていたらtrue</returns>
		public bool IsKeyDown(Keys key) {

			return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
		}
	}
	
}
