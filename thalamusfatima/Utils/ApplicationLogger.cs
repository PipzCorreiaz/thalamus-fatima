using System;
using System.IO;

namespace ThalamusFAtiMA.Utils
{
    public class ApplicationLogger
    {
        private static ApplicationLogger appLogger = new ApplicationLogger();

        public static string LOG_DIR = "logs/";
        private const string LOG_FILE = "_application_log.txt";
        private string subDir = "fatima/";
        private static StreamWriter sWriter;        
        
        private ApplicationLogger()
        {
            //sWriter = File.CreateText(LOG_FILE); // If file exists it is overriden
        }

        ~ApplicationLogger()
        {
            Close();
        }
		
        private void Close()
        {
            try
            {
                sWriter.Close();
            }
            catch (Exception)
            {
                //ignore
            }            
        }
        
        public static ApplicationLogger Instance()
        {
            return appLogger;
        }
		
		//Henrique Campos made this to work on all OS (maybe) ..
		public void CreateFile(){
			string dir = LOG_DIR;
			
			if(!Directory.Exists(dir)){
				Directory.CreateDirectory(dir);
			}
			
			if(subDir != ""){
				dir = dir + subDir;
				
				if(!Directory.Exists(dir)){
					Directory.CreateDirectory(dir);
				}
			}
			
            string dateTimeStr = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + DateTime.Now.Hour + "_" +
                                     DateTime.Now.Minute + "_" + DateTime.Now.Second;
			
            sWriter = File.CreateText(dir + dateTimeStr + LOG_FILE);
		}
        
		// Henrique Campos also changed this ..
        public void Write(string message)
        {
            if ((sWriter == null) || (sWriter.BaseStream == null) || (!sWriter.BaseStream.CanWrite))
            {
                CreateFile();
            }            
            sWriter.Write(message);
            sWriter.Flush();
        }
		
		// Henrique Campos also changed this ..
        public void WriteLine(string message)
        {
            if ((sWriter == null) || (sWriter.BaseStream == null) || (!sWriter.BaseStream.CanWrite))
            {
                CreateFile();
            }  
            sWriter.WriteLine(message);
            sWriter.Flush();
        }
        
        public void SetSubDir(String subDir)
        {
            Close(); //New file will be created in the new dir
            this.subDir = subDir;
        }
    }
}
