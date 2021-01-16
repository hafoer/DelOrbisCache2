/*********************************************
 *  Author:       		Herbert Peter
 *  Erstelldatum:       25.10.2020
 *  Letzte Änderung:	16.01.2021
 *
 *  - FunKtion: ORBIS Cache löschen
 *	- Die aktuellen und letzten ORBIS Cache-Ordner der verschiedenen Datenbanken werden von hier ausgelesen: "\\cts.mbh\dfs\Repository\log\hp\DelOrbisCache2.ini"
 *	- Diese Ordner werden nicht auf dem PC/Server gelöscht
 *	- Allen anderen ORBIS Cache-Ordner werden gelöscht.
 *	- Ergebnisse werden in Logdateien im Ordner "H:\Log" gespeichert.
 *	- Für jeden PC/Server wird eine eigene Logdatei erstellt. Dateiname: "DelOrbisCache_COMPUTERNAME.txt"
 */

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Diagnostics;


namespace DelOrbisCache2
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class App : Application
    {
        //Zuerst wird diese Funktion aufgerufen, um die Startparameter zu überprüfen
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();
            //Wurde der Parameter "auto" beim Start übergeben?
            if (e.Args.Length == 1 && e.Args[0] == "auto")
            {
                //Ja. Die Form wird nicht angezeigt. Es wird sofort mit dem Prüf- und Löschvorgang begonnen.
                //MessageBox.Show("Parameter '" + e.Args[0] + "' wurde übergeben.");

                MainWindow mw = new MainWindow();
                mw.CmdAusfuehren_Click(null, null);

                //Anwendung beenden
                Application.Current.Shutdown();
            }
            else
            {
                //Die Form zum Einstellen der Werte anzeigen.
                wnd.Show();
            }
        }
    }

    public partial class MainWindow : Window
    {
        // Konstante für Programminfos
        public const string strProgInfo = "ORBIS-Cacheordner auf Terminalserver und lokalen PCs suchen, prüfen, löschen.\n\nStand: 16.01.2021, HP";
        public MainWindow()
        {
            InitializeComponent();

            // ToolTips festlegen
            ImgInfo.ToolTip = "Programminfos anzeigen...";
            ImgPfeil.ToolTip = "Ordnername von Neu nach Letzter kopieren...";
            CmdAusfuehren.ToolTip = "Cacheordner durchsuchen und überflüssige Ordner löschen...";
            CmdBeenden.ToolTip = "Programm beenden...";
            CmdSpeichern.ToolTip = "Daten in Ini-Datei speichern und anschließend das Programm beenden...";

            //Statusanzeige aktualisieren
            TxtBoxStatus.Text = "Warten...\n";
            //TxtBoxStatus.Text = "Status: 2 Warten...\n" + TxtBoxStatus.Text;
            //TxtBoxStatus.Text = "Status: 3 Warten...\n" + TxtBoxStatus.Text;
            //TxtBoxStatus.Text = "Status: 4 Warten...\n" + TxtBoxStatus.Text;
            //TxtBoxStatus.Text = "Status: 5 Warten...\n" + TxtBoxStatus.Text;
            //TxtBoxStatus.Text = "Status: 6 Warten...\n" + TxtBoxStatus.Text;
            //TxtBoxStatus.Text = "Status: 7 Warten...\n" + TxtBoxStatus.Text;

            // INI-Datei öffnen
            //var MyIni = new IniFile("DelOrbisCache2.ini");
            var MyIni = new IniFile(@"F:\Sicherung\Entwicklung\VS2019\DelOrbisCache2\DelOrbisCache2.ini");
            //var MyIni = new IniFile(@"H:\DelOrbisCache2\DelOrbisCache2.ini");

            // INI-Datei einlesen und Werte den Textfeldern zuweisen
            TxtKHVNeu.Text = MyIni.Read("StrKHVNeu", "DelOrbisCache2");
            TxtKHVLetzter.Text = MyIni.Read("StrKHVLetzter", "DelOrbisCache2");
            TxtEDU.Text = MyIni.Read("StrEDU", "DelOrbisCache2");
            TxtTEST.Text = MyIni.Read("StrTEST", "DelOrbisCache2");
            TxtBAK.Text = MyIni.Read("StrBAK", "DelOrbisCache2");
            TxtOrdnerLokal.Text = MyIni.Read("StrOrdnerLokal", "DelOrbisCache2");
            TxtOrdnerTS.Text = MyIni.Read("StrOrdnerTS", "DelOrbisCache2");
        }

        private void CmdBeenden_Click(object sender, RoutedEventArgs e)
        {
            //Die Abbrechen-Schaltfläche wurde angeklickt
            Application.Current.Shutdown();
        }

        public void CmdAusfuehren_Click(object sender, RoutedEventArgs e)
        {
            //Die Ausführen-Schaltfläche wurde angeklickt
            //Mauszeiger als Sanduhr anzeigen
            this.Cursor = Cursors.Wait;
            //Statusanzeige aktualisieren
            TxtBoxStatus.Text = "Ordner überprüfen...\n" + TxtBoxStatus.Text;
            //string[] dirs = Directory.GetDirectories(@"F:\Sicherung\Entwicklung\VS2019\DelOrbisCache2\ORBIS-Cache", "*", SearchOption.TopDirectoryOnly);
            string[] dirs;
            //Existiert der Ordner für eine lokale ORBIS-Installation?
            var strInstallation = "";
            if (!Directory.Exists(TxtOrdnerLokal.Text))
            {
                //Nein. Existiert der Ordner für eine TS-Installation?
                if (!Directory.Exists(TxtOrdnerTS.Text))
                {
                    //Nein. Funktion beenden!
                    //Console.WriteLine("Auf diesem PC/Server gibt es keine ORBIS Cache-Ordner. Funktion wird beendet.");
                    //MessageBox.Show("Auf diesem PC / Server gibt es keine ORBIS Cache-Ordner. \nFunktion wird beendet.", "Fertig");
                    //Mauszeiger wieder auf standard setzen
                    this.Cursor = Cursors.Arrow;
                    //Statusanzeige aktualisieren
                    TxtBoxStatus.Text = "Warten...\n" + TxtBoxStatus.Text;
                    return;
                }
                else
                {
                    //Statusanzeige aktualisieren
                    TxtBoxStatus.Text = "Ordner von Terminalserver einlesen...\n" + TxtBoxStatus.Text;
                    strInstallation = TxtOrdnerTS.Text;
                    //Ordner der TS-Installation einlesen.
                    dirs = Directory.GetDirectories(TxtOrdnerTS.Text, "*", SearchOption.TopDirectoryOnly);
                }
            }
            else
            {
                //Statusanzeige aktualisieren
                TxtBoxStatus.Text = "Lokale Ordner einlesen...\n" + TxtBoxStatus.Text;
                //Der ORBIS Cache-Ordner für eine lokale Installation existiert.
                strInstallation = TxtOrdnerLokal.Text;
                //Ordner des lokalen Cache-Ordner einlesen.
                dirs = Directory.GetDirectories(TxtOrdnerLokal.Text, "*", SearchOption.TopDirectoryOnly);
            }
            int iAnz = 0;

            foreach (string dir in dirs)
            {
                //Console.WriteLine(dir);
                //Statusanzeige aktualisieren
                TxtBoxStatus.Text = "Ordner prüfen...\n" + TxtBoxStatus.Text;
                int lastpos = dir.LastIndexOf("\\");
                var strSuche = dir.Substring(lastpos + 1);
                if (strSuche == TxtKHVNeu.Text || strSuche == TxtKHVLetzter.Text || strSuche == TxtEDU.Text || strSuche == TxtTEST.Text || strSuche == TxtBAK.Text)
                {
                    //Diese Ordner dürfen NICHT gelöscht werden!
                    //Console.WriteLine("Benötigter Ordner <" + strSuche + "> gefunden");
                }
                else
                {
                    //Dieser Ordner darf gelöscht werden.
                    //Console.WriteLine("Überflüssiger Ordner <" + strSuche + "> wird gelöscht.");
                    //Statusanzeige aktualisieren
                    TxtBoxStatus.Text = "Ordner löschen...\n" + TxtBoxStatus.Text;
                    //Ordner löschen
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine(ex.Message);
                        MessageBox.Show(ex.Message);
                        continue;
                    }
                    catch (System.IO.DirectoryNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                        MessageBox.Show(ex.Message);
                        continue;
                    }

                    //Eintrag ins Ereignisprotokoll schreiben
                    //WriteEventlogMessage("DelOrbisCache2", "Überflüssiger ORBIS Cache-Ordner <" + strSuche + "> wurde gelöscht.", EventLogEntryType.Information);
                    //Eintrag in Logdatei schreiben
                    var strLogDatei = "DelOrbisCache_" + Environment.MachineName + ".txt";
                    using (StreamWriter w = File.AppendText(strLogDatei))
                    {
                        Log("Überflüssiger ORBIS Cache-Ordner <" + strInstallation + "\\" + strSuche + "> wurde gelöscht.", w);
                    }

                    using (StreamReader r = File.OpenText(strLogDatei))
                    {
                        DumpLog(r);
                    }
                    iAnz++;
                }
            }
            //Thread.Sleep(5000);
            //Console.WriteLine("Fertig! Es wurden " + iAnz + " Ordner gelöscht.");
            //MessageBox.Show("Es wurden " + iAnz + " Ordner gelöscht.", "Fertig");
            //Statusanzeige aktualisieren
            TxtBoxStatus.Text = "Warten...\n" + TxtBoxStatus.Text;
            //Mauszeiger wieder auf standard setzen
            this.Cursor = Cursors.Arrow;
        }


        //Infos für die Logdatei in den Stream schreiben
        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }

        //Infos aus dem Stream in die Logdatei schreiben
        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }



        /// schreibt einen Eintrag ins Eventlog von Windows NT/2000/XP
        /// <param name="source">Quelle</param>
        /// <param name="message">Nachricht</param>
        /// <param name="type">Typ</param>
        private void WriteEventlogMessage(string source, string message, System.Diagnostics.EventLogEntryType type)
        {
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, "MyNewLog");
            }
            System.Diagnostics.EventLog myEV = new System.Diagnostics.EventLog();
            myEV.Source = source;
            myEV.WriteEntry(message, type);
            myEV.Close();
        }

        private void CmdSpeichern_Click(object sender, RoutedEventArgs e)
        {
            //Die Speichern-Schaltfläche wurde angeklickt
            //Statusanzeige aktualisieren
            TxtBoxStatus.Text = "Parameter in die INI-Datei speichern...\n" + TxtBoxStatus.Text;


            //INI-Datei öffnen
            var MyIniSp = new IniFile(@"H:\DelOrbisCache2\DelOrbisCache2.ini");

            //Werte in INI-Datei speichern
            // Write(string Key, string Value, string Section = null)
            MyIniSp.Write("StrKHVNeu", TxtKHVNeu.Text);
            MyIniSp.Write("StrKHVLetzter", TxtKHVLetzter.Text);
            MyIniSp.Write("StrEDU", TxtEDU.Text);
            MyIniSp.Write("StrTEST", TxtTEST.Text);
            MyIniSp.Write("StrBAK", TxtBAK.Text);
            MyIniSp.Write("StrOrdnerLokal", TxtOrdnerLokal.Text);
            MyIniSp.Write("StrOrdnerTS", TxtOrdnerTS.Text);

            //Anwendung beenden
            Application.Current.Shutdown();
        }

        private void ImgInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ///Statusanzeige aktualisieren
            TxtBoxStatus.Text = "Programminfos anzeigen...\n" + TxtBoxStatus.Text;

            MessageBox.Show(strProgInfo, "Programm-Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImgPfeil_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Statusanzeige aktualisieren
            TxtBoxStatus.Text = "Inhalt vom Feld <KHV Neu> in Feld <KHV Letzter> kopiert...\n" + TxtBoxStatus.Text;

            TxtKHVLetzter.Text = TxtKHVNeu.Text;
        }
    }

    class IniFile   // Klasse zum lesen und schreiben von INI-Files
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            //Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
            Path = new FileInfo(IniPath).FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
