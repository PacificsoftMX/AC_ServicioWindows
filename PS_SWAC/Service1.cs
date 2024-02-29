using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows;
using System.Threading;
using System.Data.Common;
using System.Windows.Forms;

namespace PS_SWAC
{
    public partial class PS_BEADCOLORS : ServiceBase
    {

        System.Timers.Timer lee;
        string path = "";
        public PS_BEADCOLORS()
        {
            InitializeComponent();
            lee = new System.Timers.Timer();
            lee.Interval = 1000 * 5;//SE MULTIPLICA POR SEGUNDOS
            lee.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerTick);

        }

        Clases.funcion funcion = new Clases.funcion();
        protected override void OnStart(string[] args)
        {
            lee.Enabled = true;
        }

        #region funciones
        bool estatusTimer = false;
        public void OnTimerTick(object sender, EventArgs e)
        {
            lee.Stop();
            
            
            //if (!estatusTimer)
            //{
            //    estatusTimer = true;
                #region nuevo
            try
            {
                //funcion.cargaArchvios();

                if (p1 != null && p1.ThreadState == System.Threading.ThreadState.Stopped)
                { p1 = null; }
                if (p2 != null && p2.ThreadState == System.Threading.ThreadState.Stopped)
                { p2 = null; }
                if (p3 != null && p3.ThreadState == System.Threading.ThreadState.Stopped)
                { p3 = null; }
                //if (p4 != null && p4.ThreadState == System.Threading.ThreadState.Stopped)
                //{ p4 = null; }
                //if (p5 != null && p5.ThreadState == System.Threading.ThreadState.Stopped)
                //{ p5 = null; }
                //if (p6 != null && p6.ThreadState == System.Threading.ThreadState.Stopped)
                //{ p6 = null; }
                //if (p7 != null && p7.ThreadState == System.Threading.ThreadState.Stopped)
                //{ p7 = null; }
                //if (p8 != null && p8.ThreadState == System.Threading.ThreadState.Stopped)
                //{ p8 = null; }
                //if (p9 != null && p9.ThreadState == System.Threading.ThreadState.Stopped)
                //{ p9 = null; }
                //if (p10 != null && p10.ThreadState == System.Threading.ThreadState.Stopped)
                //{ p10 = null; }
                if (p11 != null && p11.ThreadState == System.Threading.ThreadState.Stopped)
                { p11 = null; }
                if (p12 != null && p12.ThreadState == System.Threading.ThreadState.Stopped)
                { p12 = null; }
                if (p13 != null && p13.ThreadState == System.Threading.ThreadState.Stopped)
                { p13 = null; }

                if (p1 == null)// || p2 == null || p3 == null || p4 == null || p5 == null || p6 == null || p7 == null || p8 == null || p12 == null )//|| p10 == null || p11 == null || p12 == null)//)
                    DescomprimeArchivo("SELECT TOP 5 Nombre_Paquete, fecha FROM tblAC_Paquetes WHERE Envio_Recep = 2 OR Envio_Recep = 6 Order by Fecha;");//TU CODIGO AQUI que quiereas ejecutar.

                if (p13 == null)
                { p13 = new Thread(new ThreadStart(hilo13)); p13.Start(); }
            }
            catch (Exception err)
            {
                escribe(1, "error " + err.Message + " - " + err.InnerException , ruta1 + @"\\log.txt");
                //MessageBox.Show(err.Message);
            }
                #endregion
            //estatusTimer = false;
            //}
            lee.Start();
        }

        
        #endregion


        #region Hilos
        bool Entra = false;
        public static Thread p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13;
        DataTable tabla = new DataTable(), tabla1 = new DataTable(), tabla2 = new DataTable(), tabla3 = new DataTable(), tabla4 = new DataTable(), tabla5 = new DataTable(), tabla6 = new DataTable(), tabla7 = new DataTable(), tabla8 = new DataTable(), tabla9 = new DataTable(), tabla10 = new DataTable(), tabla11 = new DataTable(), tabla12 = new DataTable();// tabla7, tabla8, tabla9;
        DataTable tablaHistorial = new DataTable();
        public static DataTable tablaHistorialMalos = new DataTable();
        string ruta = "", rutan1 = "", rutan2 = "", rutan3 = "", rutan4 = "", rutan5 = "", rutan6 = "", rutan7 = "", rutan8 = "", rutan9 = "", rutan10 = "", rutan11 = "", rutan12 = "", rutan13 = "", cnn = "";
        string ruta1 = "";
        int renglon = 0;
        private DataTable cargaDatos(DataTable tabla, DataTable llenaTabla)
        {
            Clases.funcionBD fBD = new Clases.funcionBD();
            renglon = 0; string sentencia;
            if (llenaTabla.Rows.Count == 0 && tabla.Rows.Count != 0)
            {
                if (tabla.Rows.Count > 5)
                    renglon = 5;
                else
                    renglon = tabla.Rows.Count;

                for (int i = 0; i < renglon; i++)
                {
                    llenaTabla.Rows.Add(tabla.Rows[0]["Nombre_Paquete"].ToString());
                    sentencia = "UPDATE tblac_paquetes Set Envio_Recep = 98 WHERE Nombre_Paquete = '" + tabla.Rows[0]["Nombre_Paquete"].ToString() + "' AND Envio_Recep <> 6";
                    fBD.FunicionEjecucion(sentencia);
                    //tablaHistorial.Rows.Add(tabla.Rows[0]["Nombre_Paquete"].ToString());
                    tabla.Rows.RemoveAt(0);
                }
            }
            return tabla;
        }


        private void cargaDatosM(DataTable tabla)
        {
            Clases.funcionBD fBD = new Clases.funcionBD();
            renglon = tabla.Rows.Count; string sentencia;

            for (int i = 0; i < renglon; i++)
            {
                sentencia = "UPDATE tblac_paquetes Set Envio_Recep = 95 WHERE Nombre_Paquete = '" + tabla.Rows[0]["Nombre_Paquete"].ToString() + "' AND Envio_Recep <> 6";
                fBD.FunicionEjecucion(sentencia);
            }
        }
        bool paq_98a2 = false;
        public void escribe(int numero, string dato, string archivo)
        {
            //StreamWriter writer = File.AppendText(archivo);
            StreamWriter writer1 = File.AppendText(archivo);
            do
            {
                try
                {
                    writer1 = File.AppendText(archivo);
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(250);
                }
            } while (writer1 == null);
            try
            {
                string fileName = archivo;
                // esto inserta texto en un archivo existente, si el archivo no existe lo crea

                switch (numero)
                {
                    case 1:
                        writer1.WriteLine(dato);
                        writer1.WriteLine("");
                        break;

                    case 2:
                        writer1.WriteLine("");
                        writer1.WriteLine("");
                        break;

                    case 3:

                        writer1.WriteLine(dato);
                        break;
                }
                writer1.Close();
                //writer1.Dispose();
            }
            catch
            {
                writer1.WriteLine("Error");
            }
        }

        private void DescomprimeArchivo(string sentencia)
        {
            Entra = false;
            #region Base de datos
            ruta1 = Application.StartupPath;
            if(!File.Exists(ruta1 + @"\\log.txt"))
            {
            StreamWriter writer = File.CreateText(ruta1 + @"\\log.txt");
            writer.Close();
            }
            string idEmpresa = "";
            Clases.funcionBD fBD = new Clases.funcionBD();
            Clases.funcion funciones = new Clases.funcion();
            //fBD.conexionD = fBD.ConexionDelfin();
            //idEmpresa = funciones.LeerArchivoINI("VARIOS", "EMPRESA", ruta1);
            //fBD.conexionAC = fBD.ConexionBD(idEmpresa, "CONX_AC");
            //fBD.conexionPV = fBD.ConexionBD(idEmpresa, "CONEXION");
            cnn = "";// fBD.conexionPV.ConnectionString;

            //escribe(1, fBD.conexionAC.ConnectionString, ruta1 + @"\\log.txt");
            //escribe(1, fBD.conexionPV.ConnectionString, ruta1 + @"\\log.txt");
            //escribe(1, cnn, ruta1 + @"\\log.txt");
            
            //escribe(1, "1", ruta1 + @"\\log.txt");
            if (!paq_98a2)
            {
                fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 2 where Envio_Recep = 98 OR Envio_Recep = 97");
                fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 96 where Envio_Recep = 95");
                paq_98a2 = true;
            }
            if (Clases.funcionBD.mensaje.Length > 0)
            {
                escribe(1, "1.1 " + Clases.funcionBD.mensaje, ruta1 + @"\\log.txt");
                Clases.funcionBD.mensaje = "";
            }
            #endregion
            if (ruta.Length == 0)
            {
                ruta = funciones.ReemplazarCadena(fBD.BuscaRegistroConVariasCondiciones("SELECT Ruta_Paquetes FROM tblAC_Config"), "/", @"\"); // ;
            }
            if (Clases.funcionBD.mensaje.Length > 0)
            {
                escribe(1, "1.2 " + Clases.funcionBD.mensaje, ruta1 + @"\\log.txt");
                Clases.funcionBD.mensaje = "";
            }
            //if (tabla.Rows.Count == 0)
            //{
            //escribe(1, "2 - " + ruta, ruta1 + @"\\log.txt");
            tabla = fBD.ObtieneDatosParaDataTableH1(sentencia);
            fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 2 where Envio_Recep = 87");
            //escribe(1, "3", ruta1 + @"\\log.txt");
            // tabla2 = tabla.Clone(); tabla3 = tabla.Clone();
            //tabla4 = tabla.Clone(); tabla5 = tabla.Clone(); tabla6 = tabla.Clone();
            //tabla7 = tabla.Clone(); tabla8 = tabla.Clone(); tabla9 = tabla.Clone();
            //}
            //escribe(1, "contrador " + tabla.Rows.Count.ToString(), ruta1 + @"\\log.txt");
            if (tabla.Rows.Count > 0)
            {

                #region llena tablas
                if (tabla1.Rows.Count == 0)
                { tabla1 = tabla.Clone(); tabla = cargaDatos(tabla, tabla1); }
                //if (tabla2.Rows.Count == 0)
                //{ tabla2 = tabla.Clone(); tabla = cargaDatos(tabla, tabla2); }
                //if (tabla3.Rows.Count == 0)
                //{ tabla3 = tabla.Clone(); tabla = cargaDatos(tabla, tabla3); }
                //if (tabla4.Rows.Count == 0)
                //{ tabla4 = tabla.Clone(); tabla = cargaDatos(tabla, tabla4); }
                //if (tabla5.Rows.Count == 0)
                //{ tabla5 = tabla.Clone(); tabla = cargaDatos(tabla, tabla5); }
                //if (tabla6.Rows.Count == 0)
                //{ tabla6 = tabla.Clone(); tabla = cargaDatos(tabla, tabla6); }
                //if (tabla7.Rows.Count == 0)
                //{ tabla7 = tabla.Clone(); tabla = cargaDatos(tabla, tabla7); }
                //if (tabla8.Rows.Count == 0)
                //{ tabla8 = tabla.Clone(); tabla = cargaDatos(tabla, tabla8); }
                //if (tabla9.Rows.Count == 0)
                //{ tabla9 = tabla.Clone(); tabla = cargaDatos(tabla, tabla9); }
                //if (tabla11.Rows.Count == 0)
                //{ tabla10 = tabla.Clone(); tabla = cargaDatos(tabla, tabla10); }
                //if (tabla11.Rows.Count == 0)
                //{ tabla11 = tabla.Clone(); tabla = cargaDatos(tabla, tabla11); }
                //if (tabla12.Rows.Count == 0)
                //{ tabla12 = fBD.ObtieneDatosParaDataTableH1("SELECT TOP 10 Nombre_Paquete, fecha FROM tblAC_Paquetes WHERE Envio_Recep = 97 Order by Fecha;"); }//tabla12 = tabla.Clone(); tabla = cargaDatos(tabla, tabla12, fBD.conexionAC); }
                #endregion
                #region inicia hilo
                if (p1 == null)
                { p1 = new Thread(new ThreadStart(hilo1)); p1.Start(); }
                //if (p2 == null)
                //{ p2 = new Thread(new ThreadStart(hilo2)); p2.Start(); }
                //if (p3 == null)
                //{ p3 = new Thread(new ThreadStart(hilo3)); p3.Start(); }
                //if (p4 == null)
                //{ p4 = new Thread(new ThreadStart(hilo4)); p4.Start(); }
                //if (p5 == null)
                //{ p5 = new Thread(new ThreadStart(hilo5)); p5.Start(); }
                //if (p6 == null)
                //{ p6 = new Thread(new ThreadStart(hilo6)); p6.Start(); }
                //if (p7 == null)
                //{ p7 = new Thread(new ThreadStart(hilo7)); p7.Start(); }
                //if (p8 == null)
                //{ p8 = new Thread(new ThreadStart(hilo8)); p8.Start(); }
                //if (p9 == null)
                //{ p9 = new Thread(new ThreadStart(hilo9)); p9.Start(); }
                //if (p10 == null)
                //{ p10 = new Thread(new ThreadStart(hilo10)); p10.Start(); }
                //if (p11 == null)
                //{ p11 = new Thread(new ThreadStart(hilo11)); p11.Start(); }
                //if (p12 == null && tabla12.Rows.Count > 0)
                //{ p12 = new Thread(new ThreadStart(hilo12)); p12.Start(); }
                //if (p13 == null)
                //{ p13 = new Thread(new ThreadStart(hilo13)); p13.Start(); }
                #endregion
            }
            else
            {
                if (tabla1.Rows.Count == 0)
                {
                    tabla1 = fBD.ObtieneDatosParaDataTableH1("SELECT TOP 1 Nombre_Paquete, fecha FROM tblAC_Paquetes WHERE Envio_Recep = 96 Order by Fecha;"); //tabla12 = tabla.Clone(); tabla = cargaDatos(tabla, tabla12, fBD.conexionAC); }
                    cargaDatosM(tabla1);
                    if (p1 == null && tabla1.Rows.Count > 0)
                    { p1 = new Thread(new ThreadStart(hilo1)); p1.Start(); }
                }
            }
            if (tabla11.Rows.Count == 0)
            {
                tabla11 = fBD.ObtieneDatosParaDataTableH1("SELECT TOP 5 Nombre_Paquete, fecha FROM tblAC_Paquetes WHERE Envio_Recep = 96 Order by Fecha;"); //tabla12 = tabla.Clone(); tabla = cargaDatos(tabla, tabla12, fBD.conexionAC); }
                if (p11 == null && tabla11.Rows.Count > 0)
                { cargaDatosM(tabla11); p11 = new Thread(new ThreadStart(hilo11)); p11.Start(); }
            }
            if (tabla12.Rows.Count == 0)
            {
                tabla12 = fBD.ObtieneDatosParaDataTableH1("SELECT TOP 5 Nombre_Paquete, fecha FROM tblAC_Paquetes WHERE Envio_Recep = 97 Order by Fecha;"); //tabla12 = tabla.Clone(); tabla = cargaDatos(tabla, tabla12, fBD.conexionAC); }
                if (p12 == null && tabla12.Rows.Count > 0)
                { p12 = new Thread(new ThreadStart(hilo12)); p12.Start(); }
            }

            if (tabla2.Rows.Count == 0)//Lee paquetes de replicacion
            {
                tabla2 = fBD.ObtieneDatosParaDataTableH1("SELECT Nombre_Paquete, fecha FROM tblAC_Paquetes WHERE Envio_Recep = 22 Order by Fecha;"); //tabla12 = tabla.Clone(); tabla = cargaDatos(tabla, tabla12, fBD.conexionAC); }
                if (p2 == null && tabla2.Rows.Count > 0)
                { p2 = new Thread(new ThreadStart(hilo2)); p2.Start(); }
            }

            if (tabla3.Rows.Count == 0)//Lee paquetes de replicacion
            {
                tabla3 = fBD.ObtieneDatosParaDataTableH1("SELECT Nombre_Paquete, fecha FROM tblAC_Paquetes WHERE Envio_Recep = 32 Order by Fecha;"); //tabla12 = tabla.Clone(); tabla = cargaDatos(tabla, tabla12, fBD.conexionAC); }
                if (p3 == null && tabla3.Rows.Count > 0)
                { p3 = new Thread(new ThreadStart(hilo3)); p3.Start(); }
            }
        }

        #region hilos
        private void hilo1()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla1.Rows.Count > 0)
            {
                archivo("1");
                try
                {
                    lee.lectura(tabla1, rutan1, cnn, ruta, 1);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan1);
                    lee.revisapaquetes(tabla1, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla1.Rows.Count; i > 0; i--)
                    {
                        tabla1.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p1.Abort();
            }
            catch { }
        }
        private void hilo2()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla2.Rows.Count > 0)
            {
                archivo("2");
                try
                {
                    lee.lecturaExistencia(tabla2, rutan2, cnn, ruta, 2);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan2);
                    lee.revisapaquetes(tabla2, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla2.Rows.Count; i > 0; i--)
                    {
                        tabla2.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p2.Abort();
            }
            catch { }
        }
        private void hilo3()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla3.Rows.Count > 0)
            {
                archivo("3");
                try
                {
                    lee.lecturaReplica(tabla3, rutan3, cnn, ruta, 3);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan3);
                    lee.revisapaquetes(tabla3, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla3.Rows.Count; i > 0; i--)
                    {
                        tabla3.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p3.Abort();
            }
            catch { }
        }
        private void hilo4()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla4.Rows.Count > 0)
            {
                archivo("4");
                try
                {
                    lee.lectura(tabla4, rutan4, cnn, ruta, 4);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan4);
                    lee.revisapaquetes(tabla4, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla4.Rows.Count; i > 0; i--)
                    {
                        tabla4.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p4.Abort();
            }
            catch { }
        }
        private void hilo5()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla5.Rows.Count > 0)
            {
                archivo("5");
                try
                {
                    lee.lectura(tabla5, rutan5, cnn, ruta, 5);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan5);
                    lee.revisapaquetes(tabla5, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla5.Rows.Count; i > 0; i--)
                    {
                        tabla5.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p5.Abort();
            }
            catch { }
        }
        private void hilo6()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla6.Rows.Count > 0)
            {
                archivo("6");
                try
                {
                    lee.lectura(tabla6, rutan6, cnn, ruta, 6);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan6);
                    lee.revisapaquetes(tabla6, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla6.Rows.Count; i > 0; i--)
                    {
                        tabla6.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p6.Abort();
            }
            catch { }
        }
        private void hilo7()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla7.Rows.Count > 0)
            {
                archivo("7");
                try
                {
                    lee.lectura(tabla7, rutan7, cnn, ruta, 7);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan7);
                    lee.revisapaquetes(tabla7, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla7.Rows.Count; i > 0; i--)
                    {
                        tabla7.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p7.Abort();
            }
            catch { }
        }
        private void hilo8()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla8.Rows.Count > 0)
            {
                archivo("8");
                try
                {
                    lee.lectura(tabla8, rutan8, cnn, ruta, 8);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan8);
                    lee.revisapaquetes(tabla8, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla8.Rows.Count; i > 0; i--)
                    {
                        tabla8.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p8.Abort();
            }
            catch { }
        }
        private void hilo9()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla9.Rows.Count > 0)
            {
                archivo("9");
                try
                {
                    lee.lectura(tabla9, rutan9, cnn, ruta, 9);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan9);
                    lee.revisapaquetes(tabla9, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla9.Rows.Count; i > 0; i--)
                    {
                        tabla9.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p9.Abort();
            }
            catch { }
        }
        private void hilo10()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla10.Rows.Count > 0)
            {
                archivo("10");
                try
                {
                    lee.lectura(tabla10, rutan10, cnn, ruta, 10);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan10);
                    lee.revisapaquetes(tabla10, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla10.Rows.Count; i > 0; i--)
                    {
                        tabla10.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p10.Abort();
            }
            catch { }
        }
        private void hilo11()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla11.Rows.Count > 0)
            {
                archivo("11");
                try
                {
                    lee.lectura(tabla11, rutan11, cnn, ruta, 11);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan11);
                    lee.revisapaquetes(tabla11, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla11.Rows.Count; i > 0; i--)
                    {
                        tabla11.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p11.Abort();
            }
            catch { }
        }
        private void hilo12()
        {
            Clases.funcion lee = new Clases.funcion();
            if (tabla12.Rows.Count > 0)
            {
                archivo("12");
                try
                {
                    lee.lectura(tabla12, rutan12, cnn, ruta, 12);
                }
                catch (Exception err)
                {
                    escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan12);
                    lee.revisapaquetes(tabla12, lee.nombrepaq);
                }
                finally
                {
                    for (int i = tabla12.Rows.Count; i > 0; i--)
                    {
                        tabla12.Rows.RemoveAt(i - 1);
                    }
                }
            }
            try
            {
                p12.Abort();
            }
            catch { }
        }
        private void hilo13()
        {
            Clases.funcion lee = new Clases.funcion();
            //archivo("13");
            try
            {

                funcion.cargaArchvios();
                //lee.escribe(1, "Mensaje: " + funcion.mensaje, rutan13);
            }
            catch (Exception err)
            {
                escribe(1, "Mensaje: " + err.Message + Environment.NewLine + err.StackTrace, rutan13);
                //lee.revisapaquetes(tabla12, lee.nombrepaq);
            }

            try
            {
                p13.Abort();
            }
            catch { }
        }
        #endregion

        private void archivo(string id)
        {
            #region

            string rutaArchivo = "";
            if (!System.IO.Directory.Exists(ruta + @"\AC_Logs"))
            {
                System.IO.Directory.CreateDirectory(ruta + @"\AC_Logs");
            }
            switch (Convert.ToInt32(id))
            {
                case 1:
                    rutan1 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan1;
                    break;
                case 2:
                    rutan2 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan2;
                    break;
                case 3:
                    rutan3 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan3;
                    break;
                case 4:
                    rutan4 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan4;
                    break;
                case 5:
                    rutan5 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan5;
                    break;
                case 6:
                    rutan6 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan6;
                    break;
                case 7:
                    rutan7 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan7;
                    break;
                case 8:
                    rutan8 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan8;
                    break;
                case 9:
                    rutan9 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan9;
                    break;
                case 10:
                    rutan10 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan10;
                    break;
                case 11:
                    rutan11 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan11;
                    break;
                case 12:
                    rutan12 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan12;
                    break;
                case 13:
                    rutan13 = ruta + @"\AC_Logs\logdelectura_" + id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    rutaArchivo = rutan13;
                    break;
            }
            buscarchivo(rutaArchivo);
            #endregion
        }
        private void buscarchivo(string rutan)
        {
            //if (!File.Exists(rutan))
            //{
            //    StreamWriter writer = File.CreateText(rutan);
            //    writer.Close();
            //}
            if (!System.IO.Directory.Exists(ruta + @"\Paq_No_Procesados"))
            {
                System.IO.Directory.CreateDirectory(ruta + @"\Paq_No_Procesados");
            }

            if (!System.IO.Directory.Exists(ruta + @"\Paq_Procesados"))
            {
                System.IO.Directory.CreateDirectory(ruta + @"\Paq_Procesados");
            }
            if (!System.IO.Directory.Exists(ruta + @"\Paq_Procesados\Paquetes"))
            {
                System.IO.Directory.CreateDirectory(ruta + @"\Paq_Procesados\Paquetes");
            }
            if (!System.IO.Directory.Exists(ruta + @"\Paq_Manuales"))
            {
                System.IO.Directory.CreateDirectory(ruta + @"\Paq_Manuales");
            }
        }
        #endregion


        protected override void OnStop()
        {
            //p1.Abort(); p1 = null;
            //p2.Abort(); p2 = null;
            //p3.Abort(); p3 = null;
            //p4.Abort(); p4 = null;
            //p5.Abort(); p5 = null;
            //p6.Abort(); p6 = null;
            //p7.Abort(); p7 = null;
            //p8.Abort(); p8 = null;
            lee.Dispose();
            lee.Stop();

        }
    }
}
