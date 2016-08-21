using System;
using System.Windows.Forms;
namespace getFullPath
{
    public class ClipBoardUtility
    {
        private string _path { get; set; }

        public ClipBoardUtility(string path)
        {
            _path = path;
        }

        public void SetClipBoard()
        {
            Clipboard.SetText(_path);
        }
    }
   
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var utility = new ClipBoardUtility(args[0]);
                utility.SetClipBoard();
            }
        }
    }
}
