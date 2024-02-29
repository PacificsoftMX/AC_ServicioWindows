using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using System.Diagnostics;

namespace PS_SWAC.Clases
{
    class AC_LeeXML1 : IDisposable
    {

        public string cnnxion, mensajeSentencia, archivo;

        public int nv = 0, renglon = 0, auxcaja = 0; public string mensajeHilo = "", status = "";
        public string nombre = ""; public bool error = false;
        private int IncrementaId(string tabla, string parametroid, DbConnection conexion)
        {
            int id = 0;
            string sql = "SELECT MAX(" + parametroid + ")  FROM " + tabla;
            DbCommand BuscaId = conexion.CreateCommand();
            DbDataReader resultado;
            BuscaId.CommandText = sql;
            conexion.Open();
            resultado = BuscaId.ExecuteReader();
            while (resultado.Read())
            {
                if (resultado.GetValue(0).ToString() != "")
                {
                    id = Convert.ToInt32(resultado.GetValue(0).ToString());
                }
                else
                {
                    id = 0;
                }
            }
            conexion.Close();
            id = id + 1;
            return id;
        }

        private string CambioDeFecha(string fechax)
        {
            string date = "", x = ""; int y = 0;
            if (fechax.Length > 0)
            {
                try
                {

                    if (fechax.Length > 8)
                    {
                        string[] fech = fechax.Split(' '); date = fech[0]; 
                        x = fechax.Substring(0, 4);
                        bool isNum = int.TryParse(x, out y);
                        if (isNum)
                        {
                            #region
                            if (date.Length == 10)
                                date = date.Replace("/", "");
                            else
                            {
                                x = fechax.Substring(5, 2);
                                if (x.Contains('/'))
                                {
                                    x = date.Substring(0, 5) + "0" + date.Substring(5, date.Length - 5);
                                    date = x;
                                }
                                x = date.Substring(8);
                                if (x.Length == 1)
                                {
                                    x = date.Substring(0, 8) + "0" + date.Substring(8, date.Length - 8);
                                }
                                else
                                {
                                    x = date;
                                }
                                date = x.Replace("/", "");
                            }
                            #endregion
                        }
                        else
                        {
                            #region
                            x = date.Substring(0, 2);
                            if (x.Contains('/'))
                            {
                                x = date.Substring(2, 2);
                                if (x.Contains('/'))
                                {
                                    x = "0" + date.Substring(0, 2) + "0" + date.Substring(2, 6);
                                }
                                else
                                {
                                    x = "0" + date.Substring(0, 9);
                                }
                            }
                            else
                            {
                                x = date.Substring(3, 2);
                                if (x.Contains('/'))
                                {
                                    x = date.Substring(0, 3) + "0" + date.Substring(3, 6);
                                }
                                else
                                {
                                    x = date;
                                }
                            }
                            //if (x.Length > 5)
                            date = x;
                            date = date.Substring(6, 4) + date.Substring(3, 2) + date.Substring(0, 2);
                            #endregion
                        }
                    }
                    else
                    {
                        date = fechax;
                    }
                }
                catch (Exception err)
                {
                    escribe(1, "Fecha " + err.Message + " " + err.InnerException + "CambioDeFecha(string fechax) " + fechax + "<-->" + date, archivo);
                    funcion.mensajeDB = fechax;
                    throw new System.ArgumentException("Fecha " + err.Message + " "+ err.InnerException + "CambioDeFecha(string fechax) " + fechax + "<-->" + date); 
                }
            }
            else { date = "18000101"; }

            
            return date;
        }

        public StringBuilder mensajeArchivo = new StringBuilder();

        public void escribe(int numero, string dato, string archivo)
        {
            switch (numero)
            {
                case 1:
                    mensajeArchivo.Append(dato + Environment.NewLine);
                    break;

                case 2:
                    mensajeArchivo.Append(Environment.NewLine + Environment.NewLine);
                    break;

                case 3:
                    mensajeArchivo.Append(dato + Environment.NewLine);
                    break;
            }
            if (mensajeArchivo.ToString().Split('\n').Length > 100)
            {
                escribeArchivo(archivo);
            }
        }

        public void escribeArchivo(string archivo)
        {
            using (StreamWriter outfile =
            new StreamWriter(archivo, true))
            {
                outfile.WriteLine(mensajeArchivo);
            }
            mensajeArchivo = new StringBuilder();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            //StreamWriter writer1 = File.AppendText(archivo);
            //do
            //{
            //    try
            //    {
            //        writer1 = File.AppendText(archivo);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Threading.Thread.Sleep(100);
            //    }
            //} while (writer1 == null);
            //try
            //{
            //    string fileName = archivo;
            //    // esto inserta texto en un archivo existente, si el archivo no existe lo crea
            //    writer1.WriteLine(mensajeArchivo.ToString());
            //    writer1.Close();
            //    mensajeArchivo = new StringBuilder();
            //    //writer1.Dispose();
            //}
            //catch
            //{
            //    writer1.WriteLine("Error");
            //}
        }

        public void LeeXml(string archivoXML, string rutaPath, int hilo)
        {
            #region Base de datos
            string ruta1 = Application.StartupPath;
            string idEmpresa = "";
            funcionBD BD = new funcionBD();
            funcion funciones = new funcion();
            BD.conexionD = BD.ConexionDelfin();
            //idEmpresa = funciones.LeerArchivoINI("VARIOS", "EMPRESA", ruta1);
            //BD.conexionAC = BD.ConexionBD(idEmpresa, "CONX_AC");
            //BD.conexionPV = BD.ConexionBD(idEmpresa, "CONEXION");
            #endregion
            //DataTable dtClientes = BD.datatableBD("SELECT COD_CLI FROM tblCatClientes");
            DataTable dtVendedores = BD.datatableBD("SELECT COD_VEN FROM tblVendedores");
            DataTable dtMoneda = BD.datatableBD("SELECT COD_MON, TIP_CAM FROM tblMonedas");
            DataTable dtFormasPago = BD.datatableBD("SELECT COD_FRP  FROM tblFormasPago");
            DataTable dtImpuestos = BD.datatableBD("SELECT COD_IMP FROM tblImpuestos");
            DataTable dtCajas = BD.datatableBD("SELECT COD_CAJ FROM tblCajas");
            //DataTable dtCatArticulos = BD.datatableBD("SELECT COD1_ART FROM tblCatArticulos");
            DataTable dtCatAlmacenes = BD.datatableBD("SELECT COD_ALM FROM tblCatAlmacenes");
            DataTable dtUsuarios = BD.datatableBD("SELECT COD_USU FROM tblUsuarios");

            //PS_FuncionesVB.clsCostos objCostos = new PS_FuncionesVB.clsCostos();
            Decimal cCosPEPS;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(archivoXML);
            string nombre = archivoXML, sbandera="";
            string[] nombretxt;
            nombretxt = nombre.Split(Convert.ToChar(@"\"));
            nombretxt[0] = nombretxt[nombretxt.Length - 1].Substring(0, nombretxt[nombretxt.Length - 1].Length - 4);
            string ruta, LineaError = "" ;
            ruta = rutaPath;// BD.BuscaRegistroConVariasCondiciones("SELECT Ruta_Paquetes FROM tblac_config", BD.conexionAC);
            nombre = funciones.ReemplazarCadena(ruta, "/", @"\");
            nombre = nombre + @"\" + nombretxt[0] + ".txt";
            archivo = nombre;
            //20151104 
            string fechaTimestre = "2000-01-01"; //BD.consulta("SELECT FECFIN_TRIM FROM BitacoraHistoricos")
            DateTime fechaTrimestre = Convert.ToDateTime(fechaTimestre);
            StreamWriter writer = File.CreateText(nombre);
            writer.Close();


            XmlNodeList lista;
            int numerodedatos, numerodedatos1;

            #region variables
            bool NotaDeVenta = false;
            string[,] DatosNotaDeVenta = new string[1, 36]; ;//= new string[99999, 31];
            string[,] DatosNotaDeVenta1 = new string[1, 31]; ;// = new string[99999, 31];
            string[,] DatosNotaDeVenta2 = new string[1, 42]; ;// = new string[99999, 31];
            string[,] DatosNotaDeVenta3 = new string[1, 31]; ;// = new string[99999, 31];

            string[,] DatosNotaDeDevolucion = new string[1, 31];
            string[,] RengloNotaDeDevolucion = new string[1, 29];

            bool DatosDeFactura = false;
            string[,] DatosFactura = new string[1, 36];// = new string[1, 22];
            string[,] DatosFactura1 = new string[1, 38];//= new string[99999, 23];
            string[,] DatosFactura2 = new string[1, 3];//= new string[99999, 2];
            string[,] DatosFactura3 = new string[1, 4];//= new string[99999, 4];

            bool DatosDePedidos = false;
            string[,] DatosPedidos1 = new string[1, 31];//= new string[99999, 31];
            string[,] DatosPedidos2 = new string[1, 15];//= new string[99999, 15];

            string[,] DatosAuxiliarCaja = new string[1, 44];//= new string[99999, 33];

            bool DatosDeCartera = false;
            string[,] DatosCartera = new string[1,44];//= new string[99999, 23];
            string[,] DatosCartera1 = new string[1, 10];

            bool DatosDeAlmacen = false;
            string[,] datos = new string[1, 12];
            string[,] datos1 = new string[1, 23];//= new string[99999, 23];

            string[] foliosN = new string[999999];
            String[,] FOCATI = new string[1, 3];
            #endregion
            try
            {
                error = false; mensajeHilo = "";
                escribe(1, "hilo: " + hilo + " Inicio de lectura - " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"), nombre);
                escribeArchivo(nombre);
                //-------------------------------------------------------------------------------------------------------------
                #region Nota_de_Venta
                XmlNodeList Nota_de_Venta = xDoc.GetElementsByTagName("Nota_de_Venta");
                numerodedatos = Nota_de_Venta.Count;
                string clienteNV1, usuarioNV1, mesaNV1, codSucursalGral = "";
                int caja1, empresaNV1, cfn;
                cfn = 0;
                escribe(1, "Datos Generales NV", nombre);
                escribe(2, " ", nombre);
                //DatosNotaDeVenta = new string[numerodedatos, 31];
                for (int z = 0; z < numerodedatos; z++)
                {
                    lista = ((XmlElement)Nota_de_Venta[z]).GetElementsByTagName("Datos_Generales_NV");
                    #region Datos_Generales_NV
                    
                    foreach (XmlElement nodo in lista)
                    {

                        int i = 0;

                        XmlNodeList tipo_de_movimiento = nodo.GetElementsByTagName("tipo_de_movimiento");
                        XmlNodeList folioNV = nodo.GetElementsByTagName("folioNV");
                        XmlNodeList folio_general = nodo.GetElementsByTagName("folio_general");
                        XmlNodeList TIP = nodo.GetElementsByTagName("TIP");
                        XmlNodeList clienteNV = nodo.GetElementsByTagName("clienteNV");
                        XmlNodeList fechaNV = nodo.GetElementsByTagName("fechaNV");
                        XmlNodeList fecharegistroNV = nodo.GetElementsByTagName("fecharegistroNV");
                        XmlNodeList horaNV = nodo.GetElementsByTagName("horaNV");
                        XmlNodeList caja = nodo.GetElementsByTagName("caja");
                        XmlNodeList turno = nodo.GetElementsByTagName("turno");
                        XmlNodeList subtotalNV = nodo.GetElementsByTagName("subtotalNV");
                        XmlNodeList impuestoNV = nodo.GetElementsByTagName("impuestoNV");
                        XmlNodeList importe_exentoNV = nodo.GetElementsByTagName("importe_exentoNV");
                        XmlNodeList totalNV = nodo.GetElementsByTagName("totalNV");
                        XmlNodeList total_pagadoNV = nodo.GetElementsByTagName("total_pagadoNV");
                        XmlNodeList saldoNV = nodo.GetElementsByTagName("saldoNV");
                        XmlNodeList impuesto_integradoNV = nodo.GetElementsByTagName("impuesto_integradoNV");
                        XmlNodeList estatusNV = nodo.GetElementsByTagName("estatusNV");
                        XmlNodeList folio_liqNV = nodo.GetElementsByTagName("folio_liqNV");
                        XmlNodeList notasNV = nodo.GetElementsByTagName("notasNV");
                        XmlNodeList usuarioNV = nodo.GetElementsByTagName("usuarioNV");
                        XmlNodeList descuentoNV = nodo.GetElementsByTagName("descuentoNV");
                        XmlNodeList cargosNV = nodo.GetElementsByTagName("cargosNV");
                        XmlNodeList plazoNV = nodo.GetElementsByTagName("plazoNV");
                        XmlNodeList vencimientoNV = nodo.GetElementsByTagName("vencimientoNV");
                        XmlNodeList sucursalNV = nodo.GetElementsByTagName("sucursalNV");
                        XmlNodeList contabilizadaNV = nodo.GetElementsByTagName("contabilizadaNV");
                        XmlNodeList creditoNV = nodo.GetElementsByTagName("creditoNV");
                        XmlNodeList importe_creditoNV = nodo.GetElementsByTagName("importe_creditoNV");
                        XmlNodeList mesaNV = nodo.GetElementsByTagName("mesaNV");
                        XmlNodeList empresaNV = nodo.GetElementsByTagName("empresaNV");

                        sbandera = "Nota de Venta -- " + folioNV[i].InnerText;
                        DatosNotaDeVenta[0, 0] = tipo_de_movimiento[i].InnerText;
                        DatosNotaDeVenta[0, 1] = folioNV[i].InnerText;
                        DatosNotaDeVenta[0, 2] = folio_general[i].InnerText;
                        DatosNotaDeVenta[0, 3] = TIP[i].InnerText;
                        DatosNotaDeVenta[0, 4] = clienteNV[i].InnerText;
                        DatosNotaDeVenta[0, 5] = fechaNV[i].InnerText;
                        DatosNotaDeVenta[0, 6] = fecharegistroNV[i].InnerText;
                        DatosNotaDeVenta[0, 7] = horaNV[i].InnerText;
                        DatosNotaDeVenta[0, 8] = caja[i].InnerText;
                        DatosNotaDeVenta[0, 9] = turno[i].InnerText;
                        DatosNotaDeVenta[0, 10] = subtotalNV[i].InnerText;
                        DatosNotaDeVenta[0, 11] = impuestoNV[i].InnerText;
                        DatosNotaDeVenta[0, 12] = importe_exentoNV[i].InnerText;
                        DatosNotaDeVenta[0, 13] = totalNV[i].InnerText;
                        DatosNotaDeVenta[0, 14] = total_pagadoNV[i].InnerText;
                        DatosNotaDeVenta[0, 15] = saldoNV[i].InnerText;
                        DatosNotaDeVenta[0, 16] = impuesto_integradoNV[i].InnerText;
                        DatosNotaDeVenta[0, 17] = estatusNV[i].InnerText;
                        DatosNotaDeVenta[0, 18] = folio_liqNV[i].InnerText;
                        DatosNotaDeVenta[0, 19] = notasNV[i].InnerText;
                        DatosNotaDeVenta[0, 20] = usuarioNV[i].InnerText;
                        DatosNotaDeVenta[0, 21] = descuentoNV[i].InnerText;
                        DatosNotaDeVenta[0, 22] = cargosNV[i].InnerText;
                        DatosNotaDeVenta[0, 23] = plazoNV[i].InnerText;
                        DatosNotaDeVenta[0, 24] = vencimientoNV[i].InnerText;
                        DatosNotaDeVenta[0, 25] = sucursalNV[i].InnerText;
                        DatosNotaDeVenta[0, 26] = contabilizadaNV[i].InnerText;
                        DatosNotaDeVenta[0, 27] = creditoNV[i].InnerText;
                        DatosNotaDeVenta[0, 28] = importe_creditoNV[i].InnerText;
                        DatosNotaDeVenta[0, 29] = mesaNV[i].InnerText;
                        DatosNotaDeVenta[0, 30] = empresaNV[i].InnerText;

                        try
                        {
                            XmlNodeList destino = nodo.GetElementsByTagName("destino");
                            XmlNodeList entrega = nodo.GetElementsByTagName("entrega");
                            DatosNotaDeVenta[0, 31] = destino[i].InnerText;
                            DatosNotaDeVenta[0, 32] = entrega[i].InnerText;
                        }
                        catch
                        {
                            DatosNotaDeVenta[0, 31] = "";
                            DatosNotaDeVenta[0, 32] = "0";
                        }

                        try
                        {
                            XmlNodeList FACTWEB_STS = nodo.GetElementsByTagName("FACTWEB_STS");
                            XmlNodeList FACTWEB_CVE = nodo.GetElementsByTagName("FACTWEB_CVE");
                            DatosNotaDeVenta[0, 33] = FACTWEB_STS[i].InnerText;
                            DatosNotaDeVenta[0, 34] = FACTWEB_CVE[i].InnerText;
                        }
                        catch
                        {
                            DatosNotaDeVenta[0, 33] = "0";
                            DatosNotaDeVenta[0, 34] = "";
                        }

                         //20231005
                        try
                        {
                            XmlNodeList CLIENTE_MOS = nodo.GetElementsByTagName("CLIENTE_MOS");
                            DatosNotaDeVenta[0, 35] = CLIENTE_MOS[i].InnerText;
                        }
                        catch
                        {
                            DatosNotaDeVenta[0, 35] = "";
                        }   

                       


                        codSucursalGral = DatosNotaDeVenta[0, 25];


                        if (DatosNotaDeVenta[0, 4] != "PUBLIC")
                        {
                            if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosNotaDeVenta[0, 4] + "'") == "0")
                            { clienteNV1 = "PUBLIC"; escribe(3, "No existe el cliente " + DatosNotaDeVenta[0, 4] + " para la NV " + DatosNotaDeVenta[0, 1], nombre); }
                            else { clienteNV1 = DatosNotaDeVenta[0, 4]; }
                        }
                        else
                        {
                            clienteNV1 = DatosNotaDeVenta[0, 4];
                        }

                        if (dtCajas.Select("COD_CAJ = " + Convert.ToInt32(DatosNotaDeVenta[0, 8])).Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblCajas WHERE COD_CAJ =" + DatosNotaDeVenta[0, 8] + "") == "0") //20151104
                        { caja1 = 1; escribe(3, "No existe la caja " + DatosNotaDeVenta[0, 8], nombre); }
                        else
                        {
                            caja1 = Convert.ToInt32(DatosNotaDeVenta[0, 8]);
                        }

                        if (dtUsuarios.Select("COD_USU = '" + DatosNotaDeVenta[0, 20] + "'").Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosNotaDeVenta[0, 20] + "'") == "0")//20151104
                        { usuarioNV1 = "DEPURADO"; escribe(3, "No existe el usuario " + DatosNotaDeVenta[0, 20], nombre); }
                        else
                        {
                            usuarioNV1 = DatosNotaDeVenta[0, 20];
                        }


                        mesaNV1 = DatosNotaDeVenta[0, 29];

                        if (BD.consulta("SELECT COUNT(*) FROM tblEmpresa WHERE COD_EMPRESA =" + DatosNotaDeVenta[0, 30] + ";") == "0")
                        //if (BD.consulta("SELECT COUNT(*) FROM tblEmpresa WHERE COD_EMPRESA =" + Convert.ToInt32(DatosNotaDeVenta[0, 30]) + "") == "0") //20151104
                        { empresaNV1 = 1; escribe(3, "No existe la empresa " + DatosNotaDeVenta[0, 30], nombre); }
                        else
                        {
                            empresaNV1 = Convert.ToInt32(DatosNotaDeVenta[0, 30]);
                        }

                        string fecha1, fecha2, fecha3, f1;

                        //escribe(3, DatosNotaDeVenta[0, 5] + " ---- " + DatosNotaDeVenta[0, 6] + " ---- " + DatosNotaDeVenta[0, 24], nombre);
                        fecha1 = CambioDeFecha(DatosNotaDeVenta[0, 5]);// DatosNotaDeVenta[0, 5].Substring(6, 4) + DatosNotaDeVenta[0, 5].Substring(3, 2) + DatosNotaDeVenta[0, 5].Substring(0, 2);
                        fecha2 = CambioDeFecha(DatosNotaDeVenta[0, 6]);//DatosNotaDeVenta[0, 6].Substring(6, 4) + DatosNotaDeVenta[0, 6].Substring(3, 2) + DatosNotaDeVenta[0, 6].Substring(0, 2);
                        fecha3 = CambioDeFecha(DatosNotaDeVenta[0, 24]);//DatosNotaDeVenta[0, 24].Substring(6, 4) + DatosNotaDeVenta[0, 24].Substring(3, 2) + DatosNotaDeVenta[0, 24].Substring(0, 2);
                        f1 = CambioDeFecha(DatosNotaDeVenta[0, 5]);//DatosNotaDeVenta[0, 5].Substring(6, 4) + "-" + DatosNotaDeVenta[0, 5].Substring(3, 2) + "-" + DatosNotaDeVenta[0, 5].Substring(0, 2);
                        //escribe(3, fecha1 + " ---- " + fecha2 + " ---- " + fecha3, nombre);

                        if (true)//Convert.ToDateTime(f1) > fechaTrimestre)
                        {
                            if (BD.consulta("SELECT COUNT(REF_DOC) FROM tblGralVentas WHERE REF_DOC ='" + DatosNotaDeVenta[0, 1] + "'") == "0")
                            {
                                //if (BD.consulta("SELECT COUNT(*) FROM tblGralVentas WHERE FOL_GRL ='" + DatosNotaDeVenta[0, 2] + "'") == "0") //20151104
                                //{
                                //Datos que no vienen en el XML:
                                //CON_GRL = NVEN
                                //ENVIADO = 0

                                //Se agrego el campo CLIENTE_MOS con su default 20231005
                                BD.GuardaCambios("INSERT INTO tblGralVentas(REF_DOC, FOL_GRL, COD_CLI, FEC_DOC, FEC_REG, HORA_REG, CAJA_DOC, CAJA_TUR, SUB_DOC, IVA_DOC, IMPTO_IMPTOT, TOT_DOC, TOT_PAG, TOTAL_TIP, SAL_DOC, IMPTO_INT, STS_DOC, FOL_LIQ, NOTA, COD_USU, DES_CLI, CAR1_VEN, PLA_PAG, FEC_VENC, COD_SUCU, CONTAB, CREDITO, IMPORTE_CRED, COD_MESA, COD_EMPRESA, ENVIADO, DP_DESTINO, DP_ENTREGA, FACTWEB_STS, FACTWEB_CVE, CLIENTE_MOS) VALUES ('" + DatosNotaDeVenta[0, 1] + "', '" + DatosNotaDeVenta[0, 2] + "', '" + clienteNV1 + "', '" + fecha1 + "', '" + fecha2 + "', '" + DatosNotaDeVenta[0, 7] + "', " + Convert.ToInt64(caja1) + ", " + Convert.ToInt16(DatosNotaDeVenta[0, 9]) + ", " + verificaLongitud(DatosNotaDeVenta[0, 10]) + ", " + verificaLongitud(DatosNotaDeVenta[0, 11]) + ", " + verificaLongitud(DatosNotaDeVenta[0, 12]) + ", " + verificaLongitud(DatosNotaDeVenta[0, 13]) + ", " + verificaLongitud(DatosNotaDeVenta[0, 14]) + ", " + Convert.ToDecimal(DatosNotaDeVenta[0, 3]) + ", " + verificaLongitud(DatosNotaDeVenta[0, 15]) + ", " + Convert.ToDecimal(DatosNotaDeVenta[0, 16]) + ", " + Convert.ToInt16(DatosNotaDeVenta[0, 17]) + ", '" + DatosNotaDeVenta[0, 18] + "',  '" + DatosNotaDeVenta[0, 19] + "', '" + usuarioNV1 + "', " + Convert.ToDecimal(DatosNotaDeVenta[0, 21]) + ", " + Convert.ToDecimal(DatosNotaDeVenta[0, 22]) + ", " + Convert.ToInt64(DatosNotaDeVenta[0, 23]) + ", '" + fecha3 + "', '" + DatosNotaDeVenta[0, 25] + "', " + Convert.ToInt16(DatosNotaDeVenta[0, 26]) + ", " + Convert.ToInt16(DatosNotaDeVenta[0, 27]) + ", " + verificaLongitud(DatosNotaDeVenta[0, 28]) + ", '" + mesaNV1 + "', " + empresaNV1 + ", 0, '" + DatosNotaDeVenta[0, 31] + "', " + DatosNotaDeVenta[0, 32] + ",  " + DatosNotaDeVenta[0, 33] + ", '" + DatosNotaDeVenta[0, 34] + "', '" + DatosNotaDeVenta[0, 35] + "')");


                                NotaDeVenta = true;
                                nv = nv + 1;
                                //foliosN[cfn] = DatosNotaDeVenta[0, 1];
                                //cfn = cfn + 1;
                                //}
                                //else
                                //{
                                //    escribe(3, "Ya existe la Nota de Venta FOL_GRL " + DatosNotaDeVenta[0, 2], nombre);
                                //}

                            }
                            else
                            {
                                //escribe(3, "Ya existe la Nota de Venta REF_DOC " + DatosNotaDeVenta[0, 1], nombre);
                                NotaDeVenta = false;
                            }
                        }
                        else
                        {
                            escribe(3, "Regristro de trimestre cerrado REF_DOC " + DatosNotaDeVenta[0, 1], nombre);
                            NotaDeVenta = false;
                        }
                    }
                    //}
                    #endregion
                    if (NotaDeVenta)
                    {
                        //escribe(2, "", nombre);
                        //escribe(1, "Partidas_NV", nombre);
                        //DatosNotaDeVenta1 = new string[numerodedatos, 31];
                        //for (int z = 0; z < numerodedatos; z++)
                        //{
                        #region Partidas_NV
                        lista = ((XmlElement)Nota_de_Venta[z]).GetElementsByTagName("Partidas_NV");
                        
                        foreach (XmlElement nodo in lista)
                        {

                            int i = 0;

                            XmlNodeList folio = nodo.GetElementsByTagName("folio");
                            XmlNodeList articulo = nodo.GetElementsByTagName("articulo");
                            XmlNodeList vendedor = nodo.GetElementsByTagName("vendedor");
                            XmlNodeList almacen = nodo.GetElementsByTagName("almacen");
                            XmlNodeList cantidad = nodo.GetElementsByTagName("cantidad");
                            XmlNodeList unidad = nodo.GetElementsByTagName("unidad");
                            XmlNodeList equivalencia = nodo.GetElementsByTagName("equivalencia");
                            XmlNodeList precio_catalogo = nodo.GetElementsByTagName("precio_catalogo");
                            XmlNodeList precio_venta = nodo.GetElementsByTagName("precio_venta");
                            XmlNodeList moneda = nodo.GetElementsByTagName("moneda");
                            XmlNodeList tipo_de_cambio = nodo.GetElementsByTagName("tipo_de_cambio");
                            XmlNodeList porcentaje_descto = nodo.GetElementsByTagName("porcentaje_descto");
                            XmlNodeList descto_adicional = nodo.GetElementsByTagName("descto_adicional");
                            XmlNodeList codigo_impto1 = nodo.GetElementsByTagName("codigo_impto1");
                            XmlNodeList codigo_impto2 = nodo.GetElementsByTagName("codigo_impto2");
                            XmlNodeList importe_impto1 = nodo.GetElementsByTagName("importe_impto1");
                            XmlNodeList importe_impto2 = nodo.GetElementsByTagName("importe_impto2");
                            XmlNodeList porcentaje_impto1 = nodo.GetElementsByTagName("porcentaje_impto1");
                            XmlNodeList porcentaje_impto2 = nodo.GetElementsByTagName("porcentaje_impto2");
                            XmlNodeList importe_exento = nodo.GetElementsByTagName("importe_exento");
                            XmlNodeList devueltos = nodo.GetElementsByTagName("devueltos");
                            XmlNodeList costo_de_venta = nodo.GetElementsByTagName("costo_de_venta");
                            XmlNodeList costo_peps = nodo.GetElementsByTagName("costo_peps");
                            XmlNodeList porcentaje_comision = nodo.GetElementsByTagName("porcentaje_comision");
                            XmlNodeList importe_sindescuento = nodo.GetElementsByTagName("importe_sindescuento");
                            XmlNodeList descuento_general = nodo.GetElementsByTagName("descuento_general");
                            XmlNodeList CLAVE_OFR = nodo.GetElementsByTagName("CLAVE_OFR");
                            XmlNodeList NUM_REN = nodo.GetElementsByTagName("NUM_REN");

                            sbandera = "Nota de Venta Partida  -- " + folio[i].InnerText + "  --  " + articulo[i].InnerText;
                            DatosNotaDeVenta1[0, 0] = folio[i].InnerText;
                            DatosNotaDeVenta1[0, 1] = articulo[i].InnerText;
                            DatosNotaDeVenta1[0, 2] = vendedor[i].InnerText;
                            DatosNotaDeVenta1[0, 3] = almacen[i].InnerText;
                            DatosNotaDeVenta1[0, 4] = cantidad[i].InnerText;
                            DatosNotaDeVenta1[0, 5] = unidad[i].InnerText;
                            DatosNotaDeVenta1[0, 6] = equivalencia[i].InnerText;
                            DatosNotaDeVenta1[0, 7] = precio_catalogo[i].InnerText;
                            DatosNotaDeVenta1[0, 8] = precio_venta[i].InnerText;
                            DatosNotaDeVenta1[0, 9] = moneda[i].InnerText;
                            DatosNotaDeVenta1[0, 10] = tipo_de_cambio[i].InnerText;
                            DatosNotaDeVenta1[0, 11] = porcentaje_descto[i].InnerText;
                            DatosNotaDeVenta1[0, 12] = descto_adicional[i].InnerText;
                            DatosNotaDeVenta1[0, 13] = codigo_impto1[i].InnerText;
                            DatosNotaDeVenta1[0, 14] = codigo_impto2[i].InnerText;
                            DatosNotaDeVenta1[0, 15] = importe_impto1[i].InnerText;
                            DatosNotaDeVenta1[0, 16] = importe_impto2[i].InnerText;
                            DatosNotaDeVenta1[0, 17] = porcentaje_impto1[i].InnerText;
                            DatosNotaDeVenta1[0, 18] = porcentaje_impto2[i].InnerText;
                            DatosNotaDeVenta1[0, 19] = importe_exento[i].InnerText;
                            DatosNotaDeVenta1[0, 20] = devueltos[i].InnerText;
                            DatosNotaDeVenta1[0, 21] = costo_de_venta[i].InnerText;
                            DatosNotaDeVenta1[0, 22] = costo_peps[i].InnerText;
                            DatosNotaDeVenta1[0, 23] = porcentaje_comision[i].InnerText;
                            DatosNotaDeVenta1[0, 24] = importe_sindescuento[i].InnerText;
                            DatosNotaDeVenta1[0, 25] = descuento_general[i].InnerText;

                            //20150320 validacion que se puso, para que procese paquetes antes de la actualizacion
                            try
                            {
                                DatosNotaDeVenta1[0, 26] = CLAVE_OFR[i].InnerText;
                                DatosNotaDeVenta1[0, 27] = NUM_REN[i].InnerText;
                            }
                            catch
                            {
                                DatosNotaDeVenta1[0, 26] = "";
                                DatosNotaDeVenta1[0, 27] = "0";
                            }

                            #region //20231005
                            try
                            {

                                XmlNodeList IMP0_ART = nodo.GetElementsByTagName("IMP0_ART");
                                DatosNotaDeVenta1[0, 28] = IMP0_ART[i].InnerText;
                            }
                            catch
                            {
                                DatosNotaDeVenta1[0, 28] = "0";
                            }
                            try
                            {

                                XmlNodeList IMP0_REG = nodo.GetElementsByTagName("IMP0_REG");
                                DatosNotaDeVenta1[0, 29] = IMP0_REG[i].InnerText;
                            }
                            catch
                            {
                                DatosNotaDeVenta1[0, 29] = "0";
                            }
                            try
                            {

                                XmlNodeList COD0_IMP = nodo.GetElementsByTagName("COD0_IMP");
                                DatosNotaDeVenta1[0, 30] = COD0_IMP[i].InnerText;
                            }
                            catch
                            {
                                DatosNotaDeVenta1[0, 30] = "0";
                            }
                            #endregion

                            #region Lee numero de lote y fehca de caducidad
                            XmlNodeList NUM_LOT = nodo.GetElementsByTagName("NUM_LOT");
                            XmlNodeList FEC_CAD = nodo.GetElementsByTagName("FEC_CAD");

                            #endregion

                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //    if (foliosN[r] != "" && foliosN[r] != null)
                            //    {
                            //        if (foliosN[r] == DatosNotaDeVenta1[0, 0])
                            //        {
                            string articuloN;
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblCatArticulos WHERE COD1_ART ='" + DatosNotaDeVenta1[0, 1] + "'") == "0")
                            {
                                articuloN = "DEPURADO";
                                escribe(3, "No existe el artículo " + DatosNotaDeVenta1[0, 1], nombre); //20151104
                                //“No existe el artículo” + <articulo> DatosNotaDeVenta1[0, 1]
                            }
                            else
                            {
                                articuloN = DatosNotaDeVenta1[0, 1];
                            }

                            string vendedorN;

                            if (dtVendedores.Select("COD_VEN = '" + DatosNotaDeVenta1[0, 2] + "'").Count() == 0)
                            {
                                if (BD.consulta("SELECT COUNT(*) FROM tblVendedores WHERE COD_VEN ='" + DatosNotaDeVenta1[0, 2] + "'") == "0")
                                {
                                    vendedorN = "PISO"; //“No existe el vendedor” + <vendedor> DatosNotaDeVenta1[0, 2]
                                    escribe(3, "No existe el vendedor " + DatosNotaDeVenta1[0, 2], nombre);
                                }
                                else
                                {
                                    vendedorN = DatosNotaDeVenta1[0, 2];
                                }
                            }
                            else
                            {
                                vendedorN = DatosNotaDeVenta1[0, 2];
                            }

                            string almacenN;
                            //20160703   dijo que lo quitara la validación
                            if (dtCatAlmacenes.Select("COD_ALM = '" + DatosNotaDeVenta1[0, 3] + "'").Count() == 0)
                            {
                                almacenN = ConfigurationSettings.AppSettings["Almacen"].ToString();
                                escribe(3, "No existe el almacen" + DatosNotaDeVenta1[0, 3], nombre);//“No existe el almacen” + <almacen>. DatosNotaDeVenta1[0, 3]
                            }
                            else
                            {
                                almacenN = DatosNotaDeVenta1[0, 3];
                            }
                            decimal unidadN;
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloN + "' AND COD_UND ='" + DatosNotaDeVenta1[0, 5] + "'") == "0")
                            {
                                unidadN = 1;
                                escribe(3, "No coincide la equivalencia del articulo " + DatosNotaDeVenta1[0, 1] + " para la unidad " + DatosNotaDeVenta1[0, 5], nombre);//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[0,6]
                            }
                            else
                            {
                                unidadN = Convert.ToDecimal(DatosNotaDeVenta1[0, 6]);
                            }

                            Int16 monedaN;
                            //20160703   dijo que lo quitara la validación
                            if (dtMoneda.Select("COD_MON = " + Convert.ToInt32(DatosNotaDeVenta1[0, 9])).Count() == 0)
                            {
                                monedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[0, 9]
                                escribe(3, "No existe la moneda" + DatosNotaDeVenta1[0, 9] + " de la nota de venta " + DatosNotaDeVenta1[0, 0], nombre);//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[0,6]
                            }
                            else
                            {
                                monedaN = Convert.ToInt16(DatosNotaDeVenta1[0, 9]);
                            }

                            int codigo_impto1N;
                            //20160703   dijo que lo quitara la validación
                            if (dtImpuestos.Select("COD_IMP = " + Convert.ToInt32(DatosNotaDeVenta1[0, 13])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosNotaDeVenta1[0, 13]) + "") == "0")
                            {
                                codigo_impto1N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[0, 13]
                                escribe(3, "No existe el impuesto" + DatosNotaDeVenta1[0, 13], nombre);//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[0,6]

                            }
                            else
                            {
                                codigo_impto1N = Convert.ToInt32(DatosNotaDeVenta1[0, 13]);
                            }

                            int codigo_impto2N;
                            //20160703   dijo que lo quitara la validación
                            if (dtImpuestos.Select("COD_IMP = " + Convert.ToInt32(DatosNotaDeVenta1[0, 14])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosNotaDeVenta1[0, 14]) + "") == "0")
                            {
                                codigo_impto2N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[0, 14]
                                escribe(3, "No existe el impuesto" + DatosNotaDeVenta1[0, 14], nombre);
                            }
                            else
                            {
                                codigo_impto2N = Convert.ToInt32(DatosNotaDeVenta1[0, 14]);

                            }

                            //Se agrego el campo COD0_IMP con su default 20231005
                            BD.GuardaCambios("INSERT INTO tblRenVentas(REF_DOC, COD1_ART, COD_VEN, COD_ALM, CAN_ART, COD_UND, EQV_UND, PCIO_UNI, PCIO_VEN, COD_MON, TIP_CAM, POR_DES, DECTO_ADI, COD1_IMP, COD2_IMP, IMP1_REG, IMP2_REG, IMP1_ART, IMP2_ART, IMPTO_IMP, CAN_DEV, COS_VEN, COS_PEPS, POR_COM, FOL_GRL, IMP_SINDESC, DCTO_GRAL, NUM_LOT, FEC_CAD, CLAVE_OFR, NUM_REN, IMP0_ART, IMP0_REG, COD0_IMP) VALUES('" + DatosNotaDeVenta1[0, 0] + "', '" + articuloN + "', '" + vendedorN + "', '" + almacenN + "', " + Convert.ToDecimal(DatosNotaDeVenta1[0, 4]) + ", '" + DatosNotaDeVenta1[0, 5] + "', " + Convert.ToDecimal(unidadN) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[0, 7]) + ", " + verificaLongitud(DatosNotaDeVenta1[0, 8]) + ", " + monedaN + ", " + Convert.ToDouble(DatosNotaDeVenta1[0, 10]) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[0, 11]) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[0, 12]) + ", " + Convert.ToInt16(codigo_impto1N) + ", " + Convert.ToInt16(codigo_impto2N) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[0, 15]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[0, 16]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[0, 17]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[0, 18]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[0, 19]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[0, 20]) + ",  " + verificaLongitud(DatosNotaDeVenta1[0, 21]) + ",  " + verificaLongitud(DatosNotaDeVenta1[0, 22]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[0, 23]) + ", '" + DatosNotaDeVenta1[0, 0] + "',  " + verificaLongitud(DatosNotaDeVenta1[0, 24]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[0, 25]) + ", '"+ NUM_LOT[i].InnerText +"', '" + FEC_CAD[i].InnerText + "',  '" + DatosNotaDeVenta1[0, 26] + "', " + DatosNotaDeVenta1[0, 27] + ", " + DatosNotaDeVenta1[0, 28] + ", " + DatosNotaDeVenta1[0, 29] + ", " + DatosNotaDeVenta1[0, 30] + ")");//Corregido 15-abr-2013.. Tenía comilla al final y no la tenía en el z, 0
                            renglon = renglon + 1;
                        }
                        //        }
                        //    }
                        //}
                        //}

                        #endregion
                        //escribe(2, "", nombre);
                        //escribe(1, "Pagos_NV", nombre);
                        //DatosNotaDeVenta2 = new string[numerodedatos, 31];
                        //for (int z = 0; z < numerodedatos; z++)
                        //{
                        #region Pagos_NV
                        lista = ((XmlElement)Nota_de_Venta[z]).GetElementsByTagName("Pagos_NV");
                        foreach (XmlElement nodo in lista)
                        {

                            int i = 0;

                            XmlNodeList folio = nodo.GetElementsByTagName("folio");
                            XmlNodeList folio_general = nodo.GetElementsByTagName("folio_general");
                            XmlNodeList referencia_general = nodo.GetElementsByTagName("referencia_general");
                            XmlNodeList referencia_adicional = nodo.GetElementsByTagName("referencia_adicional");
                            XmlNodeList concepto = nodo.GetElementsByTagName("concepto");
                            XmlNodeList concepto_de_caja = nodo.GetElementsByTagName("concepto_de_caja");
                            XmlNodeList concepto_general = nodo.GetElementsByTagName("concepto_general");
                            XmlNodeList caja = nodo.GetElementsByTagName("caja");
                            XmlNodeList turno = nodo.GetElementsByTagName("turno");
                            XmlNodeList usuario = nodo.GetElementsByTagName("usuario");
                            XmlNodeList autoriza = nodo.GetElementsByTagName("autoriza");
                            XmlNodeList hora = nodo.GetElementsByTagName("hora");
                            XmlNodeList código_forma = nodo.GetElementsByTagName("código_forma");
                            XmlNodeList importe_pago = nodo.GetElementsByTagName("importe_pago");
                            XmlNodeList moneda_pago = nodo.GetElementsByTagName("moneda_pago");
                            XmlNodeList tipo_de_cambio = nodo.GetElementsByTagName("tipo_de_cambio");
                            XmlNodeList importe_pago_MN = nodo.GetElementsByTagName("importe_pago_MN");
                            XmlNodeList porcentaje_cargo = nodo.GetElementsByTagName("porcentaje_cargo");
                            XmlNodeList importe_cargo = nodo.GetElementsByTagName("importe_cargo");
                            XmlNodeList referencia_pago = nodo.GetElementsByTagName("referencia_pago");
                            XmlNodeList saldo = nodo.GetElementsByTagName("saldo");
                            XmlNodeList moneda_cambio = nodo.GetElementsByTagName("moneda_cambio");
                            XmlNodeList importe_cambio = nodo.GetElementsByTagName("importe_cambio");
                            XmlNodeList tc_cambio = nodo.GetElementsByTagName("tc_cambio");
                            XmlNodeList numero_de_movimiento = nodo.GetElementsByTagName("numero_de_movimiento");
                            XmlNodeList corte_virtual = nodo.GetElementsByTagName("corte_virtual");
                            XmlNodeList corte_parcial = nodo.GetElementsByTagName("corte_parcial");
                            XmlNodeList corte_final = nodo.GetElementsByTagName("corte_final");
                            XmlNodeList contabilizado = nodo.GetElementsByTagName("contabilizado");
                            XmlNodeList sucursal = nodo.GetElementsByTagName("sucursal");
                            XmlNodeList cuenta_pago = nodo.GetElementsByTagName("cuenta_pago");
                            XmlNodeList COD_TERMINAL = nodo.GetElementsByTagName("COD_TERMINAL");
                            XmlNodeList COD_TCPROMO = nodo.GetElementsByTagName("COD_TCPROMO");
                            //** 8.1.0 .. 2017-09-12
                            XmlNodeList RFC_ORDEN = nodo.GetElementsByTagName("RFC_ORDEN");
                            XmlNodeList CUENTA_ORDEN = nodo.GetElementsByTagName("CUENTA_ORDEN");
                            XmlNodeList RFC_BENEF = nodo.GetElementsByTagName("RFC_BENEF");
                            XmlNodeList CUENTA_BENEF = nodo.GetElementsByTagName("CUENTA_BENEF");
                            XmlNodeList TIPCAD_PAGO = nodo.GetElementsByTagName("TIPCAD_PAGO");
                            XmlNodeList CERT_PAGO = nodo.GetElementsByTagName("CERT_PAGO");
                            XmlNodeList CAD_PAGO = nodo.GetElementsByTagName("CAD_PAGO");
                            XmlNodeList SELLO_PAGO = nodo.GetElementsByTagName("SELLO_PAGO");
                            XmlNodeList BANCO_ORDEN = nodo.GetElementsByTagName("BANCO_ORDEN");
                            //-- 8.1.0 .. 2017-09-12

                            
                            sbandera = "Pago de Venta Partida  -- " + folio[i].InnerText;
                            DatosNotaDeVenta2[0, 0] = folio[i].InnerText;
                            DatosNotaDeVenta2[0, 1] = folio_general[i].InnerText;
                            DatosNotaDeVenta2[0, 2] = referencia_general[i].InnerText;
                            DatosNotaDeVenta2[0, 3] = referencia_adicional[i].InnerText;
                            DatosNotaDeVenta2[0, 4] = concepto[i].InnerText;
                            DatosNotaDeVenta2[0, 5] = concepto_de_caja[i].InnerText;
                            DatosNotaDeVenta2[0, 6] = concepto_general[i].InnerText;
                            DatosNotaDeVenta2[0, 7] = caja[i].InnerText;
                            DatosNotaDeVenta2[0, 8] = turno[i].InnerText;
                            DatosNotaDeVenta2[0, 9] = usuario[i].InnerText;
                            DatosNotaDeVenta2[0, 10] = autoriza[i].InnerText;
                            DatosNotaDeVenta2[0, 11] = hora[i].InnerText;
                            DatosNotaDeVenta2[0, 12] = código_forma[i].InnerText;
                            DatosNotaDeVenta2[0, 13] = importe_pago[i].InnerText;
                            DatosNotaDeVenta2[0, 14] = moneda_pago[i].InnerText;
                            DatosNotaDeVenta2[0, 15] = tipo_de_cambio[i].InnerText;
                            DatosNotaDeVenta2[0, 16] = importe_pago_MN[i].InnerText;
                            DatosNotaDeVenta2[0, 17] = porcentaje_cargo[i].InnerText;
                            DatosNotaDeVenta2[0, 18] = importe_cargo[i].InnerText;
                            DatosNotaDeVenta2[0, 19] = referencia_pago[i].InnerText;
                            DatosNotaDeVenta2[0, 20] = saldo[i].InnerText;
                            DatosNotaDeVenta2[0, 21] = moneda_cambio[i].InnerText;
                            DatosNotaDeVenta2[0, 22] = importe_cambio[i].InnerText;
                            DatosNotaDeVenta2[0, 23] = tc_cambio[i].InnerText;
                            DatosNotaDeVenta2[0, 24] = numero_de_movimiento[i].InnerText;
                            DatosNotaDeVenta2[0, 25] = corte_virtual[i].InnerText;
                            DatosNotaDeVenta2[0, 26] = corte_parcial[i].InnerText;
                            DatosNotaDeVenta2[0, 27] = corte_final[i].InnerText;
                            DatosNotaDeVenta2[0, 28] = contabilizado[i].InnerText;
                            DatosNotaDeVenta2[0, 29] = sucursal[i].InnerText;
                            DatosNotaDeVenta2[0, 30] = cuenta_pago[i].InnerText;
                            //** 8.1.0 .. 2017-09-12
                            try
                            {

                                DatosNotaDeVenta2[0, 31] = COD_TERMINAL[i].InnerText;
                                DatosNotaDeVenta2[0, 32] = COD_TCPROMO[i].InnerText;
                                DatosNotaDeVenta2[0, 33] = RFC_ORDEN[i].InnerText;
                                DatosNotaDeVenta2[0, 34] = CUENTA_ORDEN[i].InnerText;
                                DatosNotaDeVenta2[0, 35] = RFC_BENEF[i].InnerText;
                                DatosNotaDeVenta2[0, 36] = CUENTA_BENEF[i].InnerText;
                                DatosNotaDeVenta2[0, 37] = TIPCAD_PAGO[i].InnerText;
                                DatosNotaDeVenta2[0, 38] = CERT_PAGO[i].InnerText;
                                DatosNotaDeVenta2[0, 39] = CAD_PAGO[i].InnerText;
                                DatosNotaDeVenta2[0, 40] = SELLO_PAGO[i].InnerText;
                                DatosNotaDeVenta2[0, 41] = BANCO_ORDEN[i].InnerText;
                            }
                            catch
                            {
                                DatosNotaDeVenta2[0, 31] = "0";
                                DatosNotaDeVenta2[0, 32] = "0";
                                DatosNotaDeVenta2[0, 33] = "";
                                DatosNotaDeVenta2[0, 34] = "";
                                DatosNotaDeVenta2[0, 35] = "";
                                DatosNotaDeVenta2[0, 36] = "";
                                DatosNotaDeVenta2[0, 37] = "";
                                DatosNotaDeVenta2[0, 38] = "";
                                DatosNotaDeVenta2[0, 39] = "";
                                DatosNotaDeVenta2[0, 40] = "";
                                DatosNotaDeVenta2[0, 41] = "";
                            }
                            //-- 8.1.0 .. 2017-09-12


                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //    if (foliosN[r] != "" && foliosN[r] != null)
                            //    {
                            //        if (foliosN[r] == DatosNotaDeVenta1[0, 0])
                            //        {
                            Int16 cajaNV;
                            //20160703   dijo que lo quitara la validación
                            if (dtCajas.Select("COD_CAJ = " + Convert.ToInt32(DatosNotaDeVenta2[0, 7])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblCajas WHERE COD_CAJ =" + Convert.ToInt32(DatosNotaDeVenta2[0, 7]) + "") == "0")
                            {
                                cajaNV = 1;
                            }
                            else
                            {
                                cajaNV = Convert.ToInt16(DatosNotaDeVenta2[0, 7]);
                            }

                            string codigoformaNV;
                            //20160703   dijo que lo quitara la validación
                            if (dtUsuarios.Select("COD_USU = '" + DatosNotaDeVenta2[0, 9] + "'").Count() == 0)
                            {
                                codigoformaNV = "DEPURADO"; //“No existe la forma de pago” + <código forma>.
                            }
                            else
                            {
                                codigoformaNV = DatosNotaDeVenta2[0, 9];
                            }

                            string AUTusuarioNV123;
                            //20160703   dijo que lo quitara la validación
                            if (dtUsuarios.Select("COD_USU = '" + DatosNotaDeVenta2[0, 10] + "'").Count() == 0)
                            {
                                AUTusuarioNV123 = "DEPURADO";//“No existe el usuario” + <autoriza>  DatosNotaDeVenta2[0, 22], 
                            }
                            else
                            {
                                AUTusuarioNV123 = DatosNotaDeVenta2[0, 10];
                            }

                            int codigoforma123;
                            //20160703   dijo que lo quitara la validación
                            if (dtFormasPago.Select("COD_FRP = " + Convert.ToInt32(DatosNotaDeVenta2[0, 12])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblFormasPago WHERE COD_FRP =" + Convert.ToInt32(DatosNotaDeVenta2[0, 12]) + "") == "0")
                            {
                                codigoforma123 = 1;// “No existe la forma de pago” + <código forma>, 
                                escribe(3, "No existe la forma de pago" + DatosNotaDeVenta2[0, 12], nombre);
                            }
                            else
                            {
                                codigoforma123 = Convert.ToInt32(DatosNotaDeVenta2[0, 12]);
                            }


                            Int16 monedaNV11;
                            //20160703   dijo que lo quitara la validación
                            if (dtMoneda.Select("COD_MON = " + Convert.ToInt32(DatosNotaDeVenta2[0, 14])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosNotaDeVenta2[0, 14]) + "") == "0")
                            {
                                monedaNV11 = 1; //No existe la moneda de pago” + <moneda>.
                                escribe(3, "No existe la moneda de pago" + DatosNotaDeVenta2[0, 14], nombre);
                            }
                            else
                            {
                                monedaNV11 = Convert.ToInt16(DatosNotaDeVenta2[0, 14]);
                            }

                            Int16 monedacambio123;
                            //20160703   dijo que lo quitara la validación
                            if (dtMoneda.Select("COD_MON = " + Convert.ToInt32(DatosNotaDeVenta2[0, 21])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosNotaDeVenta2[0, 21]) + "") == "0")
                            {
                                monedacambio123 = 1; //No existe la moneda de pago” + <moneda>.
                                escribe(3, "No existe el cambio de moneda" + DatosNotaDeVenta2[0, 21], nombre);
                            }
                            else
                            {
                                monedacambio123 = Convert.ToInt16(DatosNotaDeVenta2[0, 21]);
                            }

                            //** 8.1.0 .. 2017-09-09 AGREGADOS SIN VALOR .. 2017-09-12 AGREGADOS valores a los CAMPOS: RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO
                            BD.GuardaCambios("INSERT INTO tblAuxCaja(REF_DOC, FOL_GRL, REF_GRL, REF_ADI, CON_CEP, COD_CON, CON_GRL, COD_CAJ, TUR_CAJ, COD_USU, USU_AUT, HORA_DOC, COD_FRP, IMP_EXT, COD_MON, TIP_CAM, IMP_MBA, POR_CAR, IMP_CAR, REF_PAG, SAL_DOC, MON_CAMBIO, IMPE_CAMBIO, TC_CAMBIO, FOL_VIR, FOL_PAR, FOL_FIN, CONTAB, FEC_DOC, COD_CLI, ENVIADO, COD_SUCU, CTA_PAGO, FOL_COR, NOTAS, COD_TERMINAL, COD_TCPROMO,RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO, BANCO_ORDEN) VALUES ('" + DatosNotaDeVenta2[0, 0] + "', '" + DatosNotaDeVenta2[0, 1] + "', '" + DatosNotaDeVenta2[0, 2] + "', '" + DatosNotaDeVenta2[0, 3] + "', '" + DatosNotaDeVenta2[0, 4] + "', '" + DatosNotaDeVenta2[0, 5] + "', '" + DatosNotaDeVenta2[0, 6] + "', " + cajaNV + ", " + Convert.ToInt16(DatosNotaDeVenta2[0, 8]) + ", '" + codigoformaNV + "', '" + AUTusuarioNV123 + "', '" + DatosNotaDeVenta2[0, 11] + "', " + Convert.ToInt16(codigoforma123) + ", " + verificaLongitud(DatosNotaDeVenta2[0, 13]) + ", " + monedaNV11 + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 15]) + ", " + verificaLongitud(DatosNotaDeVenta2[0, 16]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 17]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 18]) + ", '" + DatosNotaDeVenta2[0, 19] + "', " + verificaLongitud(DatosNotaDeVenta2[0, 20]) + ", " + monedacambio123 + ", " + verificaLongitud(DatosNotaDeVenta2[0, 22]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 23]) + ", '" + DatosNotaDeVenta2[0, 25] + "', '" + DatosNotaDeVenta2[0, 26] + "', '" + DatosNotaDeVenta2[0, 27] + "', " + Convert.ToInt16(DatosNotaDeVenta2[0, 28]) + ", '" + BD.consulta("SELECT CONVERT(VARCHAR(10), FEC_DOC, 112) FROM tblGralVentas WHERE REF_DOC ='" + DatosNotaDeVenta2[0, 0] + "'") + "', '" + BD.consulta("SELECT COD_CLI FROM tblgralventas WHERE REF_DOC ='" + DatosNotaDeVenta2[0, 0] + "'") + "', 0,'" + DatosNotaDeVenta2[0, 29] + "', '" + DatosNotaDeVenta2[0, 30] + "', '', '', " + DatosNotaDeVenta2[0, 31] + ", " + DatosNotaDeVenta2[0, 32] + ",'" + DatosNotaDeVenta2[0, 33] + "','" + DatosNotaDeVenta2[0, 34] + "','" + DatosNotaDeVenta2[0, 35] + "','" + DatosNotaDeVenta2[0, 36] + "','" + DatosNotaDeVenta2[0, 37] + "','" + DatosNotaDeVenta2[0, 38] + "','" + DatosNotaDeVenta2[0, 39] + "','" + DatosNotaDeVenta2[0, 40] + "','" + DatosNotaDeVenta2[0, 41] + "')");

                            //BD.GuardaCambios("INSERT INTO tblAuxCaja(REF_DOC, FOL_GRL, REF_GRL, REF_ADI, CON_CEP, COD_CON, CON_GRL, COD_CAJ, TUR_CAJ, COD_USU, USU_AUT, HORA_DOC, COD_FRP, IMP_EXT, COD_MON, TIP_CAM, IMP_MBA, POR_CAR, IMP_CAR, REF_PAG, SAL_DOC, MON_CAMBIO, IMPE_CAMBIO, TC_CAMBIO, FOL_VIR, FOL_PAR, FOL_FIN, CONTAB, FEC_DOC, COD_CLI, ENVIADO, COD_SUCU, CTA_PAGO, FOL_COR, NOTAS, COD_TERMINAL, COD_TCPROMO) VALUES ('" + DatosNotaDeVenta2[0, 0] + "', '" + DatosNotaDeVenta2[0, 1] + "', '" + DatosNotaDeVenta2[0, 2] + "', '" + DatosNotaDeVenta2[0, 3] + "', '" + DatosNotaDeVenta2[0, 4] + "', '" + DatosNotaDeVenta2[0, 5] + "', '" + DatosNotaDeVenta2[0, 6] + "', " + cajaNV + ", " + Convert.ToInt16(DatosNotaDeVenta2[0, 8]) + ", '" + codigoformaNV + "', '" + AUTusuarioNV123 + "', '" + DatosNotaDeVenta2[0, 11] + "', " + Convert.ToInt16(codigoforma123) + ", " + verificaLongitud(DatosNotaDeVenta2[0, 13]) + ", " + monedaNV11 + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 15]) + ", " + verificaLongitud(DatosNotaDeVenta2[0, 16]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 17]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 18]) + ", '" + DatosNotaDeVenta2[0, 19] + "', " + verificaLongitud(DatosNotaDeVenta2[0, 20]) + ", " + monedacambio123 + ", " + verificaLongitud(DatosNotaDeVenta2[0, 22]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[0, 23]) + ", '" + DatosNotaDeVenta2[0, 25] + "', '" + DatosNotaDeVenta2[0, 26] + "', '" + DatosNotaDeVenta2[0, 27] + "', " + Convert.ToInt16(DatosNotaDeVenta2[0, 28]) + ", '" + CambioDeFecha(BD.consulta("SELECT CONVERT(Char(10),FEC_DOC,126) FROM tblGralVentas WHERE REF_DOC ='" + DatosNotaDeVenta2[0, 0] + "'")) + "', '" + BD.consulta("SELECT COD_CLI FROM tblgralventas WHERE REF_DOC ='" + DatosNotaDeVenta2[0, 0] + "'") + "', 0,'" + DatosNotaDeVenta2[0, 29] + "', '" + DatosNotaDeVenta2[0, 30] + "', '', '', " + DatosNotaDeVenta2[0, 31] + ", " + DatosNotaDeVenta2[0, 32] + ")");
                            auxcaja = auxcaja + 1;
                            //Datos que no vienen en el XML:
                            //FEC_DOC =  tblGralVentas.FEC_DOC
                            //COD_CLI =  tblGralVentas.COD_CLI
                            //ENVIADO = 0
                        }
                        //        }
                        //    }
                        //}
                        #endregion


                        #region Preguntas
                        lista = ((XmlElement)Nota_de_Venta[z]).GetElementsByTagName("Respuestas_NV");
                        sbandera = "Respuestas de Venta Partida ";
                        foreach (XmlElement nodo in lista)
                        {

                            int i = 0;

                            XmlNodeList FOLIO_NV = nodo.GetElementsByTagName("FOLIO_NV");
                            XmlNodeList COD_PREG = nodo.GetElementsByTagName("COD_PREG");
                            XmlNodeList RESP_CLI = nodo.GetElementsByTagName("RESP_CLI");
                            XmlNodeList REFER_CLI = nodo.GetElementsByTagName("REFER_CLI");
                            XmlNodeList COD_SUCU = nodo.GetElementsByTagName("COD_SUCU");

                            DatosNotaDeVenta3[0, 0] = FOLIO_NV[i].InnerText;
                            DatosNotaDeVenta3[0, 1] = COD_PREG[i].InnerText;
                            DatosNotaDeVenta3[0, 2] = RESP_CLI[i].InnerText;
                            DatosNotaDeVenta3[0, 3] = REFER_CLI[i].InnerText;
                            DatosNotaDeVenta3[0, 4] = COD_SUCU[i].InnerText;


                            BD.GuardaCambios("INSERT INTO tblEncuestaResp(FOLIO_NV, COD_PREG, RESP_CLI, REFER_CLI, COD_SUCU) VALUES ('" +
                                DatosNotaDeVenta3[0, 0] + "', " + DatosNotaDeVenta3[0, 1] + ", " + DatosNotaDeVenta3[0, 2] + ", '" + DatosNotaDeVenta3[0, 3] + "', '" + DatosNotaDeVenta3[0, 4] + "');");


                        }
                        //        }
                        //    }
                        //}
                        #endregion
                    }
                }
                #endregion
                //escribeArchivo(nombre);
                //-------------------------------------------------------------------------------------------------------------
                #region Devolucion_Cliente
                XmlNodeList Devolucion_Cliente = xDoc.GetElementsByTagName("Devolucion_Cliente");
                numerodedatos = Devolucion_Cliente.Count;
                //string clienteNV1, usuarioNV1, mesaNV1, codSucursalGral = "";
                //int caja1, empresaNV1, cfn;
                cfn = 0;
                escribe(1, "Datos Devolucion Cliente", nombre);
                escribe(2, " ", nombre);
                //DatosNotaDeVenta = new string[numerodedatos, 31];
                for (int z = 0; z < numerodedatos; z++)
                {
                    lista = ((XmlElement)Devolucion_Cliente[z]).GetElementsByTagName("Datos_Generales_DC");
                    #region Datos_Generales_NV
                    foreach (XmlElement nodo in lista)
                    {

                        int i = 0;

                        XmlNodeList tipo_de_movimiento = nodo.GetElementsByTagName("tipo_de_movimiento");
                        XmlNodeList folioDC = nodo.GetElementsByTagName("folioDC");
                        XmlNodeList conceptoDC = nodo.GetElementsByTagName("conceptoDC");
                        XmlNodeList folioconceptoDC = nodo.GetElementsByTagName("folioconceptoDC");
                        XmlNodeList conceptpgralDC = nodo.GetElementsByTagName("conceptogralDC");
                        XmlNodeList folgralconceptoDC = nodo.GetElementsByTagName("folgralconceptoDC");
                        XmlNodeList tipoDC = nodo.GetElementsByTagName("tipoDC");
                        XmlNodeList referenciaDC = nodo.GetElementsByTagName("referenciaDC");
                        XmlNodeList facturaDC = nodo.GetElementsByTagName("facturaDC");
                        XmlNodeList clienteDC = nodo.GetElementsByTagName("clienteDC");
                        XmlNodeList fechaDC = nodo.GetElementsByTagName("fechaDC");
                        XmlNodeList fecharegistroDC = nodo.GetElementsByTagName("fecharegistroDC");
                        XmlNodeList horaDC = nodo.GetElementsByTagName("horaDC");
                        XmlNodeList subtotalDC = nodo.GetElementsByTagName("subtotalDC");
                        XmlNodeList impuestoDC = nodo.GetElementsByTagName("impuestoDC");
                        XmlNodeList importe_exentoDC = nodo.GetElementsByTagName("importe_exentoDC");
                        XmlNodeList totalDC = nodo.GetElementsByTagName("totalDC");
                        XmlNodeList regcargoDC = nodo.GetElementsByTagName("regcargoDC");
                        XmlNodeList impuesto_integradoDC = nodo.GetElementsByTagName("impuesto_integradoDC");
                        XmlNodeList almacenDC = nodo.GetElementsByTagName("almacenDC");
                        XmlNodeList estatusDC = nodo.GetElementsByTagName("estatusDC");
                        XmlNodeList notasDC = nodo.GetElementsByTagName("notasDC");
                        XmlNodeList usuarioDC = nodo.GetElementsByTagName("usuarioDC");
                        XmlNodeList sucursalDC = nodo.GetElementsByTagName("sucursalDC");
                        XmlNodeList contabilizadaDC = nodo.GetElementsByTagName("contabilizadaDC");
                        XmlNodeList conf_cfdi = nodo.GetElementsByTagName("conf_cfdi");



                        sbandera = "Devolucion datos generales ---  " + folioDC[i].InnerText; 
                        DatosNotaDeDevolucion[0, 0] = tipo_de_movimiento[i].InnerText;
                        DatosNotaDeDevolucion[0, 1] = folioDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 2] = conceptoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 3] = folioconceptoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 4] = conceptpgralDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 5] = folgralconceptoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 6] = tipoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 7] = referenciaDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 8] = facturaDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 9] = clienteDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 10] = fechaDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 11] = fecharegistroDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 12] = horaDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 13] = subtotalDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 14] = impuestoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 15] = importe_exentoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 16] = totalDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 17] = regcargoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 18] = impuesto_integradoDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 19] = almacenDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 20] = estatusDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 21] = notasDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 22] = usuarioDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 23] = sucursalDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 24] = contabilizadaDC[i].InnerText;
                        DatosNotaDeDevolucion[0, 25] = conf_cfdi[i].InnerText;

                        #region 20231005
                        try
                        {
                            XmlNodeList PORC_RETIVA = nodo.GetElementsByTagName("PORC_RETIVA");
                            DatosNotaDeDevolucion[0, 26] = PORC_RETIVA[i].InnerText;
                        }
                        catch
                        {
                            DatosNotaDeDevolucion[0, 26] = "0";
                        }
                        try
                        {
                            XmlNodeList IMP_RETIVA = nodo.GetElementsByTagName("IMP_RETIVA");
                            DatosNotaDeDevolucion[0, 27] = IMP_RETIVA[i].InnerText;
                        }
                        catch
                        {
                            DatosNotaDeDevolucion[0, 27] = "0";
                        }
                        try
                        {
                            XmlNodeList PORC_RETISR = nodo.GetElementsByTagName("PORC_RETISR");
                            DatosNotaDeDevolucion[0, 28] = PORC_RETISR[i].InnerText;
                        }
                        catch
                        {
                            DatosNotaDeDevolucion[0, 28] = "0";
                        }
                        try
                        {
                            XmlNodeList IMP_RETISR = nodo.GetElementsByTagName("IMP_RETISR");
                            DatosNotaDeDevolucion[0, 29] = IMP_RETISR[i].InnerText;
                        }
                        catch
                        {
                            DatosNotaDeDevolucion [0, 29] = "0";
                        }
                        #endregion

                        if (DatosNotaDeDevolucion[0, 9] != "PUBLIC")
                        {
                            if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosNotaDeDevolucion[0, 9] + "'") == "0")
                            { clienteNV1 = "PUBLIC"; escribe(3, "No existe el cliente " + DatosNotaDeDevolucion[0, 9] + " para la devolución " + DatosNotaDeDevolucion[0, 1], nombre); }
                            else { clienteNV1 = DatosNotaDeDevolucion[0, 9]; }
                        }
                        else
                        {
                            clienteNV1 = DatosNotaDeVenta[0, 4];
                        }

                        if (dtUsuarios.Select("COD_USU = '" + DatosNotaDeDevolucion[0, 22] + "'").Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosNotaDeVenta[0, 20] + "'") == "0")//20151104
                        { usuarioNV1 = "DEPURADO"; escribe(3, "No existe el usuario " + DatosNotaDeDevolucion[0, 20], nombre); }
                        else
                        {
                            usuarioNV1 = DatosNotaDeDevolucion[0, 22];
                        }

                        string almacenN = "";
                        if (dtCatAlmacenes.Select("COD_ALM = '" + DatosNotaDeDevolucion[0, 19] + "'").Count() == 0)
                        {
                            almacenN = ConfigurationSettings.AppSettings["Almacen"].ToString();
                            escribe(3, "No existe el almacen" + DatosNotaDeDevolucion[0, 19], nombre);//“No existe el almacen” + <almacen>. DatosNotaDeVenta1[0, 3]
                        }
                        else
                        {
                            almacenN = DatosNotaDeDevolucion[0, 19];
                        }

                        string fecha1, fecha2, fecha3, f1;
                        fecha1 = CambioDeFecha(DatosNotaDeDevolucion[0, 10]);// DatosNotaDeDevolucion[0, 10].Substring(6, 4) + DatosNotaDeDevolucion[0, 10].Substring(3, 2) + DatosNotaDeDevolucion[0, 10].Substring(0, 2);
                        fecha2 = CambioDeFecha(DatosNotaDeDevolucion[0, 11]);//DatosNotaDeDevolucion[0, 11].Substring(6, 4) + DatosNotaDeDevolucion[0, 11].Substring(3, 2) + DatosNotaDeDevolucion[0, 11].Substring(0, 2);
                        f1 = fecha1 = CambioDeFecha(DatosNotaDeDevolucion[0, 10]);//DatosNotaDeDevolucion[0, 10].Substring(6, 4) + "-" + DatosNotaDeDevolucion[0, 10].Substring(3, 2) + "-" + DatosNotaDeDevolucion[0, 10].Substring(0, 2);

                        if (true)//Convert.ToDateTime(f1) > fechaTrimestre)
                        {
                            if (BD.consulta("SELECT COUNT(FOL_DEV) FROM tblEncDevolucion WHERE  FOL_DEV ='" + DatosNotaDeDevolucion[0, 1] + "'") == "0")
                            {
                                //if (BD.consulta("SELECT COUNT(*) FROM tblGralVentas WHERE FOL_GRL ='" + DatosNotaDeVenta[0, 2] + "'") == "0") //20151104
                                //{
                                //Datos que no vienen en el XML:
                                //CON_GRL = NVEN
                                //ENVIADO = 0

                                //                                              1        2        3        4         5        6       7        8           9      10       11         12          13    14          15          16          17          18      19      20       21     22      23          24
                                BD.GuardaCambios("INSERT INTO tblEncDevolucion(FOL_DEV, COD_CON, FOL_CON, CON_GRL, FOL_GRL, TIP_DEV, REF_DOC, FOLIO_FACT, COD_CLI, FEC_DEV, FEC_REG, HORA_REG, SUB_DEV, IVA_DEV, IMPTO_IMPTOT, TOT_DEV, REG_CARGO, IMPTO_INT, COD_ALM, COD_STS, NOTA, COD_USU, COD_SUCU, CONTAB, CONF_CFDI, ENVIADO, PORC_RETIVA, IMP_RETIVA, PORC_RETISR, IMP_RETISR) VALUES "
                                   + " ('" + DatosNotaDeDevolucion[0, 1] + "', '" + DatosNotaDeDevolucion[0, 2] + "', '" + DatosNotaDeDevolucion[0, 3] + "', '" + DatosNotaDeDevolucion[0, 4] + "', '" + DatosNotaDeDevolucion[0, 5] + "', " + DatosNotaDeDevolucion[0, 6] + ", '" + DatosNotaDeDevolucion[0, 7] + "', '" + DatosNotaDeDevolucion[0, 8] + "', '" + DatosNotaDeDevolucion[0, 9] + "', '" + fecha1 + "', '" + fecha2 + "', '" + DatosNotaDeDevolucion[0, 12] + "', " + DatosNotaDeDevolucion[0, 13] + ", " + DatosNotaDeDevolucion[0, 14] + ", " + DatosNotaDeDevolucion[0, 15] + ", " + DatosNotaDeDevolucion[0, 16] + ", " + DatosNotaDeDevolucion[0, 17] + ", " + DatosNotaDeDevolucion[0, 18] + ",  '" + DatosNotaDeDevolucion[0, 19] + "', " + DatosNotaDeDevolucion[0, 20] + ", '" + DatosNotaDeDevolucion[0, 21] + "', '" + DatosNotaDeDevolucion[0, 22] + "', '" + DatosNotaDeDevolucion[0, 23] + "', " + DatosNotaDeDevolucion[0, 24] + ", '" + DatosNotaDeDevolucion[0, 25] + "', 0, " + DatosNotaDeDevolucion[0, 26] + ", " + DatosNotaDeDevolucion[0, 27] + ", " + DatosNotaDeDevolucion[0, 28] + ", " + DatosNotaDeDevolucion[0, 29] + ");");
                                NotaDeVenta = true;
                                //nv = nv + 1;
                                //foliosN[cfn] = DatosNotaDeVenta[0, 1];
                                //cfn = cfn + 1;
                                //}
                                //else
                                //{
                                //    escribe(3, "Ya existe la Nota de Venta FOL_GRL " + DatosNotaDeVenta[0, 2], nombre);
                                //}

                            }
                            else
                            {
                                //escribe(3, "Ya existe la Nota de Venta REF_DOC " + DatosNotaDeDevolucion[0, 1], nombre);
                                NotaDeVenta = false;
                            }
                        }
                        else
                        {
                            escribe(3, "Regristro de trimestre cerrado REF_DOC " + DatosNotaDeDevolucion[0, 1], nombre);
                            NotaDeVenta = false;
                        }
                    }
                    //}
                    #endregion
                    if (NotaDeVenta)
                    {
                        #region Partidas_DC
                        lista = ((XmlElement)Devolucion_Cliente[z]).GetElementsByTagName("Partidas_DC");

                        foreach (XmlElement nodo in lista)
                        {

                            int i = 0;

                            XmlNodeList folioRENDC = nodo.GetElementsByTagName("folioRENDC");
                            XmlNodeList foliogeneralRENDC = nodo.GetElementsByTagName("foliogeneralRENDC");
                            XmlNodeList cantidadRENDC = nodo.GetElementsByTagName("cantidadRENDC");
                            XmlNodeList articuloRENDC = nodo.GetElementsByTagName("articuloRENDC");
                            XmlNodeList sustituidoRENDC = nodo.GetElementsByTagName("sustituidoRENDC");
                            XmlNodeList unidadRENDC = nodo.GetElementsByTagName("unidadRENDC");
                            XmlNodeList precio_ventaRENDC = nodo.GetElementsByTagName("precio_ventaRENDC");
                            XmlNodeList equivalenciaRENDC = nodo.GetElementsByTagName("equivalenciaRENDC");
                            XmlNodeList costo_de_ventaRENDC = nodo.GetElementsByTagName("costo_de_ventaRENDC");
                            XmlNodeList movimientoRENDC = nodo.GetElementsByTagName("movimientoRENDC");
                            XmlNodeList importe_exentoRENDC = nodo.GetElementsByTagName("importe_exentoRENDC");
                            XmlNodeList precio_netoRENDC = nodo.GetElementsByTagName("precio_netoRENDC");
                            XmlNodeList codigo_impto1RENDC = nodo.GetElementsByTagName("codigo_impto1RENDC");
                            XmlNodeList codigo_impto2RENDC = nodo.GetElementsByTagName("codigo_impto2RENDC");
                            XmlNodeList porcentaje_impto1RENDC = nodo.GetElementsByTagName("porcentaje_impto1RENDC");
                            XmlNodeList porcentaje_impto2RENDC = nodo.GetElementsByTagName("porcentaje_impto2RENDC");
                            XmlNodeList importe_impto1RENDC = nodo.GetElementsByTagName("importe_impto1RENDC");
                            XmlNodeList importe_impto2RENDC = nodo.GetElementsByTagName("importe_impto2RENDC");

                            sbandera = "Partida Devolucion ---  " + folioRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 0] = folioRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 1] = foliogeneralRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 2] = cantidadRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 3] = articuloRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 4] = sustituidoRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 5] = unidadRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 6] = precio_ventaRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 7] = equivalenciaRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 8] = costo_de_ventaRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 9] = movimientoRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 10] = importe_exentoRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 11] = precio_netoRENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 12] = codigo_impto1RENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 13] = codigo_impto2RENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 14] = porcentaje_impto1RENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 15] = porcentaje_impto2RENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 16] = importe_impto1RENDC[i].InnerText;
                            RengloNotaDeDevolucion[0, 17] = importe_impto2RENDC[i].InnerText;

                            #region 20231005
                            try
                            {
                                XmlNodeList IMP0_ART = nodo.GetElementsByTagName("IMP0_ART");
                                RengloNotaDeDevolucion[0, 18] = IMP0_ART[i].InnerText;
                            }
                            catch
                            {
                                RengloNotaDeDevolucion[0, 18] = "0";
                            } 
                            try
                            {
                                XmlNodeList IMP0_REG = nodo.GetElementsByTagName("IMP0_REG");
                                RengloNotaDeDevolucion[0, 19] = IMP0_REG[i].InnerText;
                            }
                            catch
                            {
                                RengloNotaDeDevolucion[0, 19] = "0";
                            } 
                            try
                            {
                                XmlNodeList COD0_IMP = nodo.GetElementsByTagName("COD0_IMP");
                                RengloNotaDeDevolucion[0, 20] = COD0_IMP[i].InnerText;
                            }
                            catch
                            {
                                RengloNotaDeDevolucion[0, 20] = "0";
                            } 
                            try
                            {
                                XmlNodeList IMP_RETIVA = nodo.GetElementsByTagName("IMP_RETIVA");
                                RengloNotaDeDevolucion[0, 21] = IMP_RETIVA[i].InnerText;
                            }
                            catch
                            {
                                RengloNotaDeDevolucion[0, 21] = "0";
                            } 
                            try
                            {
                                XmlNodeList IMP_RETISR = nodo.GetElementsByTagName("IMP_RETISR");
                                RengloNotaDeDevolucion[0, 22] = IMP_RETISR[i].InnerText;
                            }
                            catch
                            {
                                RengloNotaDeDevolucion[0, 22] = "0";
                            }
                            #endregion

                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //    if (foliosN[r] != "" && foliosN[r] != null)
                            //    {
                            //        if (foliosN[r] == DatosNotaDeVenta1[0, 0])
                            //        {
                            string articuloD = "";
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblCatArticulos WHERE COD1_ART ='" + RengloNotaDeDevolucion[0, 3] + "'") == "0")
                            {
                                articuloD = "DEPURADO";
                                escribe(3, "No existe el artículo " + RengloNotaDeDevolucion[0, 3], nombre); //20151104
                                //“No existe el artículo” + <articulo> DatosNotaDeVenta1[0, 1]
                            }
                            else
                            {
                                articuloD = RengloNotaDeDevolucion[0, 3];
                            }

                            string articuloS = "";
                            if (RengloNotaDeDevolucion[0, 4].Length > 0)
                            {
                                if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblCatArticulos WHERE COD1_ART ='" + RengloNotaDeDevolucion[0, 3] + "'") == "0")
                                {
                                    articuloS = "DEPURADO";
                                    escribe(3, "No existe el artículo " + RengloNotaDeDevolucion[0, 3], nombre); //20151104
                                    //“No existe el artículo” + <articulo> DatosNotaDeVenta1[0, 1]
                                }
                                else
                                {
                                    articuloS = RengloNotaDeDevolucion[0, 3];
                                }
                            }

                            string unidadN = "";
                            if (articuloS == "DEPURADO")
                            {
                                if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloD + "' AND COD_UND ='" + RengloNotaDeDevolucion[0, 5] + "'") == "0")
                                {
                                    unidadN = BD.consulta("SELECT COD_UND FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloD + "' AND COD_UND =1;");
                                    escribe(3, "No coincide la unidad para el articulo " + articuloD, nombre);//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[0,6]
                                }
                                else
                                {
                                    unidadN = RengloNotaDeDevolucion[0, 5];
                                }
                            }
                            else
                            {
                                unidadN = RengloNotaDeDevolucion[0, 5];
                            }

                            //                                              1       2           3       4       5           6       7           8       9       10      11          12          13      14          15      16          17          18
                            BD.GuardaCambios("INSERT INTO tblRenDevolucion(FOL_DEV, FOL_GRL, CAN_DEV, COD1_ART, SUS1_ART, COD_UND, PCIO_ART, EQV_UND, COS_ART, NUM_MOV, IMPTO_IMP, PCIO_NETO, COD1_IMP, COD2_IMP, IMP1_ART, IMP2_ART, IMP1_REG, IMP2_REG, IMP0_ART, IMP0_REG, COD0_IMP, IMP_RETIVA, IMP_RETISR) VALUES " +
                                " ('" + RengloNotaDeDevolucion[0, 0] + "', '" + RengloNotaDeDevolucion[0, 1] + "', " + RengloNotaDeDevolucion[0, 2] + ", '" + articuloD + "', '" + articuloS + "', '" + unidadN + "', " + RengloNotaDeDevolucion[0, 6] + ", " + RengloNotaDeDevolucion[0, 7] + ", " + RengloNotaDeDevolucion[0, 8] + ", " + RengloNotaDeDevolucion[0, 9] + ", " + RengloNotaDeDevolucion[0, 10] + ", " + RengloNotaDeDevolucion[0, 11] + ", " + RengloNotaDeDevolucion[0, 12] + ", " + RengloNotaDeDevolucion[0, 13] + ", " + RengloNotaDeDevolucion[0, 14] + ", " + RengloNotaDeDevolucion[0, 15] + ", " + RengloNotaDeDevolucion[0, 16] + ", " + RengloNotaDeDevolucion[0, 17] + ", " + RengloNotaDeDevolucion[0, 18] + ", " + RengloNotaDeDevolucion[0, 19] + ", " + RengloNotaDeDevolucion[0, 20] + ", " + RengloNotaDeDevolucion[0, 21] + ", " + RengloNotaDeDevolucion[0, 22] + ");");//Corregido 15-abr-2013.. Tenía comilla al final y no la tenía en el z, 0

                            #region
                            string folio = "";
                            folio = BD.consulta("SELECT REF_DOC FROM tblEncDevolucion WHERE FOL_DEV = '" + RengloNotaDeDevolucion[0, 0] + "';");
                            if (BD.consulta("SELECT COUNT(REF_DOC)  FROM tblGralVentas WHERE REF_DOC = '" + folio + "';") != "0")
                            {
                                BD.GuardaCambios("UPDATE tblRenventas SET CAN_DEV=CAN_DEV + " + RengloNotaDeDevolucion[0, 2] + " WHERE REF_DOC='" + folio + "' AND NUM_MOV="+ RengloNotaDeDevolucion[0, 9] +";");
                            }
                            else
                            {
                                escribe(3, "No existe la nota de venta " + folio, nombre);
                            }
                            #endregion

                            renglon = renglon + 1;
                        }
                        //        }
                        //    }
                        //}
                        //}

                        #endregion
                    }
                }
                #endregion
                //escribeArchivo(nombre);
                //-------------------------------------------------------------------------------------------------------------
                #region Facturas
                escribe(2, "", nombre);
                escribe(2, "", nombre);
                escribe(1, "Facturas", nombre);
                //MessageBox.Show("Facturas");
                XmlNodeList Facturas = xDoc.GetElementsByTagName("Facturas");
                numerodedatos = Facturas.Count;
                //DatosFactura = new string[numerodedatos, 22];
                cfn = 0;
                for (int z = 0; z < numerodedatos; z++)
                {
                    lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Datos_Generales_Factura");
                    #region Datos_Generales_Factura
                    foreach (XmlElement nodo in lista)
                    {

                        int i = 0;

                        XmlNodeList tipo_de_movimiento = nodo.GetElementsByTagName("tipo_de_movimiento");
                        XmlNodeList foliofact = nodo.GetElementsByTagName("foliofact");
                        XmlNodeList folio_general = nodo.GetElementsByTagName("folio_general");
                        XmlNodeList concepto = nodo.GetElementsByTagName("concepto");
                        XmlNodeList clientefact = nodo.GetElementsByTagName("clientefact");
                        XmlNodeList fechafact = nodo.GetElementsByTagName("fechafact");
                        XmlNodeList subtotalfact = nodo.GetElementsByTagName("subtotalfact");
                        XmlNodeList impuestofact = nodo.GetElementsByTagName("impuestofact");
                        XmlNodeList importe_exentofact = nodo.GetElementsByTagName("importe_exentofact");
                        XmlNodeList totalfact = nodo.GetElementsByTagName("totalfact");
                        XmlNodeList impuesto_integrado_fact = nodo.GetElementsByTagName("impuesto_integrado_fact");
                        XmlNodeList tip_fact = nodo.GetElementsByTagName("tip_fact");
                        XmlNodeList estatusfact = nodo.GetElementsByTagName("estatusfact");
                        XmlNodeList notasfact = nodo.GetElementsByTagName("notasfact");
                        XmlNodeList usuariofact = nodo.GetElementsByTagName("usuariofact");
                        XmlNodeList sucursalfact = nodo.GetElementsByTagName("sucursalfact");
                        XmlNodeList descuentofact = nodo.GetElementsByTagName("descuentofact");
                        XmlNodeList cargosfact = nodo.GetElementsByTagName("cargosfact");
                        XmlNodeList vencimientofact = nodo.GetElementsByTagName("vencimientofact");
                        XmlNodeList creditofact = nodo.GetElementsByTagName("creditofact");
                        XmlNodeList importe_creditofact = nodo.GetElementsByTagName("importe_creditofact");
                        XmlNodeList empresafact = nodo.GetElementsByTagName("empresafact");
                        XmlNodeList USO_CFDI = nodo.GetElementsByTagName("USO_CFDI");
                        XmlNodeList CONF_CFDI = nodo.GetElementsByTagName("CONF_CFDI");
                        XmlNodeList HORA_FAC = nodo.GetElementsByTagName("HORA_FAC");

                        sbandera = "Factura datos generales ---  " + foliofact[i].InnerText; 
                        DatosFactura[0, 0] = tipo_de_movimiento[i].InnerText;
                        DatosFactura[0, 1] = foliofact[i].InnerText;
                        DatosFactura[0, 2] = folio_general[i].InnerText;
                        DatosFactura[0, 3] = concepto[i].InnerText;
                        DatosFactura[0, 4] = clientefact[i].InnerText;
                        DatosFactura[0, 5] = fechafact[i].InnerText;
                        DatosFactura[0, 6] = subtotalfact[i].InnerText;
                        DatosFactura[0, 7] = impuestofact[i].InnerText;
                        DatosFactura[0, 8] = importe_exentofact[i].InnerText;
                        DatosFactura[0, 9] = totalfact[i].InnerText;
                        DatosFactura[0, 10] = impuesto_integrado_fact[i].InnerText;
                        DatosFactura[0, 11] = tip_fact[i].InnerText;
                        DatosFactura[0, 12] = estatusfact[i].InnerText;
                        DatosFactura[0, 13] = notasfact[i].InnerText;
                        DatosFactura[0, 14] = usuariofact[i].InnerText;
                        DatosFactura[0, 15] = sucursalfact[i].InnerText;
                        DatosFactura[0, 16] = descuentofact[i].InnerText;
                        DatosFactura[0, 17] = cargosfact[i].InnerText;
                        DatosFactura[0, 18] = vencimientofact[i].InnerText;
                        DatosFactura[0, 19] = creditofact[i].InnerText;
                        DatosFactura[0, 20] = importe_creditofact[i].InnerText;
                        DatosFactura[0, 21] = empresafact[i].InnerText;

                        try
                        {
                            XmlNodeList destino = nodo.GetElementsByTagName("destino");
                            XmlNodeList entrega = nodo.GetElementsByTagName("entrega");
                            DatosFactura[0, 22] = destino[i].InnerText;
                            DatosFactura[0, 23] = entrega[i].InnerText;
                            DatosFactura[0, 24] = USO_CFDI[i].InnerText;
                            DatosFactura[0, 25] = CONF_CFDI[i].InnerText;
                            DatosFactura[0, 26] = HORA_FAC[i].InnerText;
                        }
                        catch
                        {

                            DatosFactura[0, 22] = "";
                            DatosFactura[0, 23] = "0";
                            DatosFactura[0, 24] = "";
                            DatosFactura[0, 25] = "";
                            DatosFactura[0, 26] = "00:00:01";
                        }

                        #region 20231005
                        try
                        {
                            XmlNodeList PORC_RETIVA = nodo.GetElementsByTagName("PORC_RETIVA");
                            DatosFactura[0, 27] = PORC_RETIVA[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 27] = "0";
                        }
                        try
                        {
                            XmlNodeList IMP_RETIVA = nodo.GetElementsByTagName("IMP_RETIVA");
                            DatosFactura[0, 28] = IMP_RETIVA[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 28] = "0";
                        }
                        try
                        {
                            XmlNodeList PORC_RETISR = nodo.GetElementsByTagName("PORC_RETISR");
                            DatosFactura[0, 29] = PORC_RETISR[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 29] = "0";
                        }
                        try
                        {
                            XmlNodeList IMP_RETISR = nodo.GetElementsByTagName("IMP_RETISR");
                            DatosFactura[0, 30] = IMP_RETISR[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 30] = "0";
                        }
                        try
                        {
                            XmlNodeList FEC_CANC = nodo.GetElementsByTagName("FEC_CANC");
                            DatosFactura[0, 31] = FEC_CANC[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 31] = "18000101";
                        }
                        try
                        {
                            XmlNodeList SUSTIT_FACT = nodo.GetElementsByTagName("SUSTIT_FACT");
                            DatosFactura[0, 32] = SUSTIT_FACT[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 32] = "18000101";
                        }
                        try
                        {
                            XmlNodeList FE_INTEIMP = nodo.GetElementsByTagName("FE_INTEIMP");
                            DatosFactura[0, 33] = FE_INTEIMP[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 33] = "18000101";
                        }
                        try
                        {
                            XmlNodeList FE_DESGIEPS = nodo.GetElementsByTagName("FE_DESGIEPS");
                            DatosFactura[0, 34] = FE_DESGIEPS[i].InnerText;
                        }
                        catch
                        {
                            DatosFactura[0, 34] = "18000101";
                        }
                        #endregion



                        //20160703   dijo que lo quitara la validación

                        string conceptoDF;
                        if (BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE COD_CON ='" + DatosFactura[0, 3] + "'") == "0")
                        {
                            conceptoDF = BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE TIP_MOV ='FACT'");
                            escribe(3, "No existe la serie de factura" + DatosFactura[0, 3] + " se asignó " + conceptoDF, nombre);
                        }
                        else
                        {
                            conceptoDF = DatosFactura[0, 3];
                        }


                        string clienteNV123;
                        if (DatosFactura[0, 4] != "PUBLIC")
                        {
                            if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosFactura[0, 4] + "'") == "0")
                            {
                                clienteNV123 = "PUBLIC";//“No existe el cliente” + <clienteNV> + “para la Factura” + <foliofact> DatosFactura[0, 3], 
                                escribe(3, "No existe el cliente " + DatosFactura[0, 4] + " para la Factura " + DatosFactura[0, 1], nombre);

                            }
                            else
                            {
                                clienteNV123 = DatosFactura[0, 4];
                            }
                        }
                        else
                        {
                            clienteNV123 = DatosFactura[0, 4];
                        }
                        string usuarioNV123;
                        if (dtUsuarios.Select("COD_USU = '" + DatosFactura[0, 14] + "'").Count() == 0)
                        {
                            if (BD.consulta("SELECT COUNT(COD_USU) FROM tblUsuarios WHERE COD_USU ='" + DatosFactura[0, 14] + "'") == "0")
                            {
                                usuarioNV123 = "DEPURADO";//“No existe el usuario” + <usuarioNV>  DatosFactura[0, 3], 
                                escribe(3, "No existe el usuario" + DatosFactura[0, 14], nombre);

                            }
                            else
                            {
                                usuarioNV123 = DatosFactura[0, 14];
                            }
                        }
                        else
                        {
                            usuarioNV123 = DatosFactura[0, 14];
                        }

                        Int16 empresaNV1234;
                        if (BD.consulta("SELECT COUNT(*) FROM tblEmpresa WHERE COD_EMPRESA =" + DatosFactura[0, 21]) == "0")
                        {
                            empresaNV1234 = 1;//“No existe la empresa” + <empresaNV>  DatosFactura[0, 21], 
                            escribe(3, "No existe la empresa " + DatosFactura[0, 21], nombre);

                        }
                        else
                        {
                            empresaNV1234 = Convert.ToInt16(DatosFactura[0, 21]);
                        }


                        if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOLIO_FAC ='" + DatosFactura[0, 1] + "'") != "0")
                        {
                            //““Ya existe la Factura” + FOLIO_FAC.> DatosFactura[0, 1]
                            //escribe(3, "Ya existe la Factura con folio " + DatosFactura[0, 1], nombre);
                            DatosDeFactura = false;
                        }
                        else
                        {
                            if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOL_GRL ='" + DatosFactura[0, 2] + "'") != "0")
                            {
                                //““Ya existe la Factura” + FOLIO_FAC.> DatosFactura[0, 2]
                                //escribe(3, "Ya existe la factura con folio general" + DatosFactura[0, 2], nombre);
                                DatosDeFactura = false;
                            }
                            else
                            {
                                escribe(3, DatosFactura[0, 5] + " --- " + DatosFactura[0, 6] + " --- " + DatosFactura[0, 7] + " --- " + DatosFactura[0, 8] + " --- " + DatosFactura[0, 9] + " --- " + DatosFactura[0, 10] + " --- " + DatosFactura[0, 11] + " --- " + DatosFactura[0, 12] + " --- " + DatosFactura[0, 15] + " --- " + DatosFactura[0, 16] + " --- " + DatosFactura[0, 17] + " --- " + DatosFactura[0, 18] + " --- " + DatosFactura[0, 20], nombre);
                                //escribeArchivo(nombre);
                                //string f1 = DatosFactura[0, 5].Substring(0, DatosFactura[0, 5].Length - 5);//.Substring(6, 4) + "-" + DatosFactura[0, 5].Substring(3, 2) + "-" + DatosFactura[0, 5].Substring(0, 2);
                                //if (Convert.ToDateTime(f1) > fechaTrimestre)//20151104

                                ////if (Convert.ToDateTime(DatosFactura[0, 5].Substring(0, 10)) > fechaTrimestre)
                                //{
                                string f1 = DatosFactura[0, 18];

                                //DateTime fec = Convert.ToDateTime(fecha.Substring(0, fecha.Length - 5));
                                f1 = CambioDeFecha(DatosFactura[0, 18]);// f1.Substring(6, 4) + "-" + f1.Substring(3, 2) + "-" + f1.Substring(0, 2);


                                string sent = "INSERT INTO tblfacturasenc(FOLIO_FAC, FOL_GRL, COD_CON, COD_CLI, FEC_FAC, SUB_DOC, IVA_DOC, IMPTO_IMPTOT, TOT_DOC, IMPTO_INT, TOTAL_TIP, STS_DOC, NOTA, COD_USU, COD_SUCU, DES_CLI, CAR1_VEN, FEC_VENC, CREDITO, IMPORTE_CRED, COD_EMPRESA, ENVIADO, HORA_FAC, FOLIO_DIG, SAT_MPAGO, CTA_PAGO, DP_DESTINO, DP_ENTREGA, USO_CFDI, CONF_CFDI, PORC_RETIVA, IMP_RETIVA, PORC_RETISR, IMP_RETISR, FEC_CANC, SUSTIT_FACT, FE_INTEIMP, FE_DESGIEPS) VALUES ('" +
                                DatosFactura[0, 1] + "', '" +
                                DatosFactura[0, 2] + "', '" +
                                conceptoDF + "', '" +
                                clienteNV123 + "', '" +
                                CambioDeFecha(DatosFactura[0, 5]) + "', " +
                                verificaLongitud(DatosFactura[0, 6]) + ", " +
                                verificaLongitud(DatosFactura[0, 7]) + ", " +
                                Convert.ToDecimal(DatosFactura[0, 8]) + ", " +
                                verificaLongitud(DatosFactura[0, 9]) + ", " +
                                Convert.ToDecimal(DatosFactura[0, 10]) + ", " +
                                Convert.ToDecimal(DatosFactura[0, 11]) + ", " +
                                Convert.ToInt16(DatosFactura[0, 12]) + ", '" +
                                DatosFactura[0, 13] + "', '" +
                                usuarioNV123 + "', '" +
                                DatosFactura[0, 15] + "', " +
                                verificaLongitud(DatosFactura[0, 16]) + ", '" +
                                verificaLongitud(DatosFactura[0, 17]) + "', '" +
                                f1 + "', " +
                                Convert.ToInt16(DatosFactura[0, 19]) + ", " +
                                verificaLongitud(DatosFactura[0, 20]) + ", " +
                                empresaNV1234 + ", 0, '00:00:00', 'S/N', '', '', '" + DatosFactura[0, 22] + "', " + DatosFactura[0, 23] + ", '" + DatosFactura[0, 24] + "', '" + DatosFactura[0, 25] + "', " + DatosFactura[0, 27] + ", " + DatosFactura[0, 28] + ", " + DatosFactura[0, 29] + ", " + DatosFactura[0, 30] + ", '" + DatosFactura[0, 31] + "', '" + DatosFactura[0, 32] + "', " + DatosFactura[0, 33] + ", " + DatosFactura[0, 34] + ")";
                                //escribe(1, sent, nombre);
                                BD.GuardaCambios(sent);

                                //Datos que no vienen en el XML:
                                // ENVIADO = 0
                                //foliosN[cfn] = DatosFactura[0, 1];
                                //cfn = cfn + 1;
                                DatosDeFactura = true;
                                //}
                                //else
                                //{
                                //    DatosDeFactura = false;
                                //    escribe(3, "Regristro de trimestre cerrado FEC_FAC " + DatosFactura[0, 5], nombre);
                                //    escribeArchivo(nombre);
                                //}
                            }
                        }
                    }
                    //}
                    #endregion
                    if (DatosDeFactura)
                    {
                        //escribe(2, "", nombre);
                        //escribe(2, "", nombre);
                        //escribe(1, "Partidas_Factura", nombre);
                        //DatosFactura1 = new string[numerodedatos, 33];
                        //for (int z = 0; z < numerodedatos; z++)
                        //{
                        lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Partidas_Factura");
                        #region Partidas_Factura
                        foreach (XmlElement nodo in lista)
                        {

                            int i = 0;

                            XmlNodeList folio = nodo.GetElementsByTagName("folio");
                            XmlNodeList articulo = nodo.GetElementsByTagName("articulo");
                            XmlNodeList cantidad = nodo.GetElementsByTagName("cantidad");
                            XmlNodeList unidad = nodo.GetElementsByTagName("unidad");
                            XmlNodeList equivalencia = nodo.GetElementsByTagName("equivalencia");
                            XmlNodeList precio_venta = nodo.GetElementsByTagName("precio_venta");
                            XmlNodeList moneda = nodo.GetElementsByTagName("moneda");
                            XmlNodeList tipo_de_cambio = nodo.GetElementsByTagName("tipo_de_cambio");
                            XmlNodeList porcentaje_descto = nodo.GetElementsByTagName("porcentaje_descto");
                            XmlNodeList descto_adicional = nodo.GetElementsByTagName("descto_adicional");
                            XmlNodeList codigo_impto1 = nodo.GetElementsByTagName("codigo_impto1");
                            XmlNodeList codigo_impto2 = nodo.GetElementsByTagName("codigo_impto2");
                            XmlNodeList porcentaje_impto1 = nodo.GetElementsByTagName("porcentaje_impto1");
                            XmlNodeList porcentaje_impto2 = nodo.GetElementsByTagName("porcentaje_impto2");
                            XmlNodeList importe_impto1 = nodo.GetElementsByTagName("importe_impto1");
                            XmlNodeList importe_impto2 = nodo.GetElementsByTagName("importe_impto2");
                            XmlNodeList importe_exento = nodo.GetElementsByTagName("importe_exento");
                            XmlNodeList numero_movimiento = nodo.GetElementsByTagName("numero_movimiento");
                            XmlNodeList importe_sindescuento = nodo.GetElementsByTagName("importe_sindescuento");//19
                            XmlNodeList descuento_general = nodo.GetElementsByTagName("descuento_general");//20

                            XmlNodeList precio_uni = nodo.GetElementsByTagName("precio_uni");//21
                            XmlNodeList fecha_cad = nodo.GetElementsByTagName("fecha_cad");//22
                            XmlNodeList numero_lot = nodo.GetElementsByTagName("numero_lot");//23
                            XmlNodeList UND_CFDI = nodo.GetElementsByTagName("UND_CFDI");//24
                            XmlNodeList FOLIO_NV = nodo.GetElementsByTagName("FOLIO_NV");//25
                            XmlNodeList CVEART_CFDI = nodo.GetElementsByTagName("CVEART_CFDI");//26 8.1.0 .. 2017-08-14

                            sbandera = "Partida facturas ---  " + folio[i].InnerText + " --- " + articulo[i].InnerText; 
                            DatosFactura1[0, 0] = folio[i].InnerText;
                            DatosFactura1[0, 1] = articulo[i].InnerText;
                            DatosFactura1[0, 2] = cantidad[i].InnerText;
                            DatosFactura1[0, 3] = unidad[i].InnerText;
                            DatosFactura1[0, 4] = equivalencia[i].InnerText;
                            DatosFactura1[0, 5] = precio_venta[i].InnerText;
                            DatosFactura1[0, 6] = moneda[i].InnerText;
                            DatosFactura1[0, 7] = tipo_de_cambio[i].InnerText;
                            DatosFactura1[0, 8] = porcentaje_descto[i].InnerText;
                            DatosFactura1[0, 9] = descto_adicional[i].InnerText;
                            DatosFactura1[0, 10] = codigo_impto1[i].InnerText;
                            DatosFactura1[0, 11] = codigo_impto2[i].InnerText;
                            DatosFactura1[0, 12] = porcentaje_impto1[i].InnerText;
                            DatosFactura1[0, 13] = porcentaje_impto2[i].InnerText;
                            DatosFactura1[0, 14] = importe_impto1[i].InnerText;
                            DatosFactura1[0, 15] = importe_impto2[i].InnerText;
                            DatosFactura1[0, 16] = importe_exento[i].InnerText;
                            DatosFactura1[0, 17] = numero_movimiento[i].InnerText;
                            DatosFactura1[0, 18] = importe_sindescuento[i].InnerText;
                            DatosFactura1[0, 19] = descuento_general[i].InnerText;
                            DatosFactura1[0, 20] = precio_uni[i].InnerText;
                            DatosFactura1[0, 21] = fecha_cad[i].InnerText;
                            DatosFactura1[0, 22] = numero_lot[i].InnerText;
                            DatosFactura1[0, 23] = UND_CFDI[i].InnerText;
                            DatosFactura1[0, 24] = FOLIO_NV[i].InnerText;
                            DatosFactura1[0, 25] = CVEART_CFDI[i].InnerText; // 8.1.0 .. 2017-08-14


                            try
                            {
                                XmlNodeList IMPINT_REN = nodo.GetElementsByTagName("IMPINT_REN");//26 8.1.0 .. 2017-08-14
                                DatosFactura1[0, 26] = IMPINT_REN[i].InnerText; // 8.5.0 .. 2018-08-2
                            }
                            catch
                            {
                                DatosFactura1[0, 26] = "0"; // 8.5.0 .. 2018-08-2
                            }

                            #region 20231005
                            try
                            {
                                XmlNodeList IMP0_ART = nodo.GetElementsByTagName("IMP0_ART");
                                DatosFactura1[0, 27] = IMP0_ART[i].InnerText;
                            }
                            catch
                            {
                                DatosFactura1[0, 27] = "0";
                            }
                            try
                            {
                                XmlNodeList IMP0_REG = nodo.GetElementsByTagName("IMP0_REG");
                                DatosFactura1[0, 28] = IMP0_REG[i].InnerText;
                            }
                            catch
                            {
                                DatosFactura1[0, 28] = "0";
                            }
                            try
                            {
                                XmlNodeList COD0_IMP = nodo.GetElementsByTagName("COD0_IMP");
                                DatosFactura1[0, 29] = COD0_IMP[i].InnerText;
                            }
                            catch
                            {
                                DatosFactura1[0, 29] = "0";
                            }
                            try
                            {
                                XmlNodeList IMP_RETIVA = nodo.GetElementsByTagName("IMP_RETIVA");
                                DatosFactura1[0, 30] = IMP_RETIVA[i].InnerText;
                            }
                            catch
                            {
                                DatosFactura1[0, 30] = "0";
                            }
                            try
                            {
                                XmlNodeList IMP_RETISR = nodo.GetElementsByTagName("IMP_RETISR");
                                DatosFactura1[0, 31] = IMP_RETISR[i].InnerText;
                            }
                            catch
                            {
                                DatosFactura1[0, 31] = "0";
                            }
                            try
                            {
                                XmlNodeList FE_OBJIMP = nodo.GetElementsByTagName("FE_OBJIMP");
                                DatosFactura1[0, 32] = FE_OBJIMP[i].InnerText;
                            }
                            catch
                            {
                                DatosFactura1[0, 32] = "0";
                            }

                            #endregion

                            //20160703   dijo que lo quitara la validación
                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //    if (foliosN[r] != "" && foliosN[r] != null)
                            //    {
                            //        if (foliosN[r] == DatosFactura1[0, 0])
                            //        {
                            string articuloN;
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblCatArticulos WHERE COD1_ART ='" + DatosFactura1[0, 1] + "'") == "0")
                            {
                                articuloN = "DEPURADO"; //No existe el artículo” + <articulo> DatosFactura1[0, 1]
                                escribe(3, "No existe el artículo" + DatosFactura1[0, 1], nombre);
                            }
                            else
                            {
                                articuloN = DatosFactura1[0, 1];
                            }

                            string unidadN;
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloN + "' AND COD_UND ='" + DatosFactura1[0, 3] + "'") == "0")
                            {
                                unidadN = "1";//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[0,3]
                                escribe(3, "No coincide la equivalencia del articulo " + DatosFactura1[0, 1] + " para la unidad " + DatosFactura1[0, 3], nombre);
                            }
                            else
                            {
                                unidadN = DatosFactura1[0, 3];
                            }  

                            Int16 monedaN;
                            if (dtMoneda.Select("COD_MON = " + Convert.ToInt32(DatosFactura1[0, 6])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosFactura1[0, 6]) + "") == "0")
                            {
                                monedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[0, 6]
                                escribe(3, "No existe la moneda" + DatosFactura1[0, 6], nombre);
                            }
                            else
                            {
                                monedaN = Convert.ToInt16(DatosFactura1[0, 6]);
                            }

                            int codigo_impto1N;
                            if (dtImpuestos.Select("COD_IMP = " + Convert.ToInt32(DatosFactura1[0, 10])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosFactura1[0, 10]) + "") == "0")
                            {
                                codigo_impto1N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[0, 10]
                                escribe(3, "No existe el impuesto " + DatosFactura1[0, 10], nombre);

                            }
                            else 
                            {
                                codigo_impto1N = Convert.ToInt32(DatosFactura1[0, 10]);
                            }

                            int codigo_impto2N;
                            if (dtImpuestos.Select("COD_IMP = " + Convert.ToInt32(DatosFactura1[0, 11])).Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosFactura1[0, 11]) + "") == "0")
                            {
                                codigo_impto2N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[0, 11]
                                escribe(3, "No existe el impuesto " + DatosFactura1[0, 11], nombre);

                            }
                            else
                            {
                                codigo_impto2N = Convert.ToInt32(DatosFactura1[0, 11]);
                            }

                            string fechacad = CambioDeFecha(DatosFactura1[0, 21]);
                            //BD.GuardaCambios("INSERT INTO tblFacturasRen(FOLIO_FAC, COD1_ART, CAN_ART, COD_UND, EQV_UND, PCIO_VEN, COD_MON, TIP_CAM, POR_DES, DECTO_ADI, COD1_IMP, COD2_IMP, IMP1_ART, IMP2_ART, IMP1_REG, IMP2_REG, IMPTO_IMP, NUM_MOV, IMP_SINDESC, DCTO_GRAL, PCIO_UNI, NUM_LOT, FEC_CAD, UND_CFDI, FOLIO_NV, CVEART_CFDI) VALUES ('" + DatosFactura1[0, 0] + "', '" + articuloN + "', " + Convert.ToDecimal(DatosFactura1[0, 2]) + ", '" + unidadN + "', " + Convert.ToDecimal(DatosFactura1[0, 4]) + ",  " + verificaLongitud(DatosFactura1[0, 5]) + ", " + monedaN + ", " + Convert.ToDouble(DatosFactura1[0, 7]) + ", " + Convert.ToDecimal(DatosFactura1[0, 8]) + ", " + Convert.ToDecimal(DatosFactura1[0, 9]) + ", " + codigo_impto1N + ", " + codigo_impto2N + ", " + Convert.ToDecimal(DatosFactura1[0, 12]) + ", " + Convert.ToDecimal(DatosFactura1[0, 13]) + ", " + verificaLongitud(DatosFactura1[0, 14]) + ", " + verificaLongitud(DatosFactura1[0, 15]) + ", " + verificaLongitud(DatosFactura1[0, 16]) + ", " + Convert.ToInt64(DatosFactura1[0, 17]) + ", " + verificaLongitud(DatosFactura1[0, 18]) + ", " + verificaLongitud(DatosFactura1[0, 19]) + ", " + verificaLongitud(DatosFactura1[0, 20]) + ", '" + DatosFactura1[0, 22] + "', '" + fechacad + "', '" + DatosFactura1[0, 23] + "', '" + DatosFactura1[0, 24] + "', '" + DatosFactura1[0, 25] + "')");
                            //Se agrego el campo FE_OBJIMP con su default 20231005
                            BD.GuardaCambios("INSERT INTO tblFacturasRen (FOLIO_FAC, COD1_ART, CAN_ART, COD_UND, EQV_UND, PCIO_VEN, COD_MON, TIP_CAM, POR_DES, DECTO_ADI, COD1_IMP, COD2_IMP, IMP1_ART, IMP2_ART, IMP1_REG, IMP2_REG, IMPTO_IMP, NUM_MOV, IMP_SINDESC, DCTO_GRAL, PCIO_UNI, NUM_LOT, FEC_CAD, UND_CFDI, FOLIO_NV, CVEART_CFDI, IMPINT_REN, IMP0_ART, IMP0_REG, COD0_IMP, IMP_RETIVA, IMP_RETISR, FE_OBJIMP) VALUES ('" + DatosFactura1[0, 0] + "', '" + articuloN + "', " + Convert.ToDecimal(DatosFactura1[0, 2]) + ", '" + unidadN + "', " + Convert.ToDecimal(DatosFactura1[0, 4]) + ",  " + verificaLongitud(DatosFactura1[0, 5]) + ", " + monedaN + ", " + Convert.ToDouble(DatosFactura1[0, 7]) + ", " + Convert.ToDecimal(DatosFactura1[0, 8]) + ", " + Convert.ToDecimal(DatosFactura1[0, 9]) + ", " + codigo_impto1N + ", " + codigo_impto2N + ", " + Convert.ToDecimal(DatosFactura1[0, 12]) + ", " + Convert.ToDecimal(DatosFactura1[0, 13]) + ", " + verificaLongitud(DatosFactura1[0, 14]) + ", " + verificaLongitud(DatosFactura1[0, 15]) + ", " + verificaLongitud(DatosFactura1[0, 16]) + ", " + Convert.ToInt64(DatosFactura1[0, 17]) + ", " + verificaLongitud(DatosFactura1[0, 18]) + ", " + verificaLongitud(DatosFactura1[0, 19]) + ", " + verificaLongitud(DatosFactura1[0, 20]) + ", '" + DatosFactura1[0, 22] + "', '" + fechacad + "', '" + DatosFactura1[0, 23] + "', '" + DatosFactura1[0, 24] + "', '" + DatosFactura1[0, 25] + "', " + DatosFactura1[0, 26] + ", " + DatosFactura1[0, 27] + ", " + DatosFactura1[0, 28] + ", " + DatosFactura1[0, 29] + ", " + DatosFactura1[0, 30] + ", " + DatosFactura1[0, 31] + ", " + DatosFactura1[0, 32] + ")");
                            //NUM_MOV = consecutivo (incremento)
                            //        }
                            //    }
                            //}
                        }
                        #endregion
                        //}
                        //----------------------------------------------------------------------------------------------------------
                        DatosFactura2 = new string[numerodedatos, 3];
                        //for (int z = 0; z < numerodedatos; z++)
                        //{
                        lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Folio_NV");
                        #region Folio_NV
                        foreach (XmlElement nodo in lista)
                        {
                            int i = 0;

                            XmlNodeList folioNV = nodo.GetElementsByTagName("folioNV");
                            XmlNodeList folioFAC = nodo.GetElementsByTagName("folioFAC");
                            XmlNodeList totalNV = nodo.GetElementsByTagName("totalNV");

                            sbandera = "Folio_NV ---  " + folioNV[i].InnerText; 
                            
                            DatosFactura2[0, 0] = folioNV[i].InnerText;
                            DatosFactura2[0, 1] = folioFAC[i].InnerText;
                            DatosFactura2[0, 2] = totalNV[i].InnerText;

                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //    if (foliosN[r] != "" && foliosN[r] != null)
                            //    {
                            //        if (foliosN[r] == DatosFactura2[0, 0])
                            //        {
                            BD.GuardaCambios("INSERT INTO tblnotasporfactura(FOLIO_NV, FOLIO_FACT, TOTAL_NV) VALUES ('" + DatosFactura2[0, 0] + "', '" + DatosFactura2[0, 1] + "', " + DatosFactura2[0, 2] + ")");
                            //        }
                            //    }
                            //}
                        }
                        #endregion
                        //}

                        //----------------------------------------------------------------------------------------------------------
                        DatosFactura3 = new string[numerodedatos, 4];
                        //for (int z = 0; z < numerodedatos; z++)
                        //{
                        lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Folio_Descripcion");
                        #region Folio_Descripcion
                        foreach (XmlElement nodo in lista)
                        {
                            int i = 0;

                            XmlNodeList FOLIO = nodo.GetElementsByTagName("FOLIO");
                            XmlNodeList COD_ART = nodo.GetElementsByTagName("COD_ART");
                            XmlNodeList NUM_MOV = nodo.GetElementsByTagName("NUM_MOV");
                            XmlNodeList DESC_ART = nodo.GetElementsByTagName("DESC_ART");

                            sbandera = "Folio_Descripcion ---  " + FOLIO[i].InnerText; 
                            DatosFactura3[0, 0] = FOLIO[i].InnerText;
                            DatosFactura3[0, 1] = COD_ART[i].InnerText;
                            DatosFactura3[0, 2] = NUM_MOV[i].InnerText;
                            DatosFactura3[0, 3] = DESC_ART[i].InnerText;

                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //    if (foliosN[r] != "" && foliosN[r] != null)
                            //    {
                            //        if (foliosN[r] == DatosFactura3[0, 0])
                            //        {
                            int numero;
                            if (DatosFactura3[0, 2] != "" && DatosFactura3[0, 2] != null)
                            {
                                numero = Convert.ToInt32(DatosFactura3[0, 2]);
                            }
                            else
                            {
                                numero = 0;
                            }
                            BD.GuardaCambios("INSERT INTO tblFeDescripciones(FOLIO, COD_ART, NUM_MOV, DESC_ART) VALUES ('" + DatosFactura3[0, 0] + "', '" + DatosFactura3[0, 1] + "', " + numero + ", '" + DatosFactura3[0, 3] + "')");
                            //        }
                            //    }
                            //}
                        }
                        #endregion
                    }
                }
                #endregion
                //escribeArchivo(nombre);
                //-------------------------------------------------------------------------------------------------------------
                #region Pedidos
                escribe(2, "", nombre);
                escribe(2, "", nombre);
                escribe(1, "Pedidos", nombre);
                XmlNodeList Pedidos = xDoc.GetElementsByTagName("Pedidos");

                numerodedatos = Pedidos.Count;
                //DatosPedidos1 = new string[numerodedatos, 31];
                cfn = 0;
                for (int z = 0; z < numerodedatos; z++)
                {
                    lista = ((XmlElement)Pedidos[z]).GetElementsByTagName("Datos_Generales_Pedido");
                    #region Datos_Generales_Pedido
                    foreach (XmlElement nodo in lista)
                    {

                        int i = 0;

                        XmlNodeList tipo_de_movimiento = nodo.GetElementsByTagName("tipo_de_movimiento");
                        XmlNodeList folio_pedido = nodo.GetElementsByTagName("folio_pedido");
                        XmlNodeList fecha_pedido = nodo.GetElementsByTagName("fecha_pedido");
                        XmlNodeList hora_pedido = nodo.GetElementsByTagName("hora_pedido");
                        XmlNodeList tipo_pedido = nodo.GetElementsByTagName("tipo_pedido");
                        XmlNodeList referencia_pedido = nodo.GetElementsByTagName("referencia_pedido");
                        XmlNodeList docto_de_referencia = nodo.GetElementsByTagName("docto_de_referencia");
                        XmlNodeList cliente_pedido = nodo.GetElementsByTagName("cliente_pedido");
                        XmlNodeList ruta_pedido = nodo.GetElementsByTagName("ruta_pedido");
                        XmlNodeList vendedor_pedido = nodo.GetElementsByTagName("vendedor_pedido");
                        XmlNodeList almacen_pedido = nodo.GetElementsByTagName("almacen_pedido");
                        XmlNodeList subtotal_pedido = nodo.GetElementsByTagName("subtotal_pedido");
                        XmlNodeList impuesto_pedido = nodo.GetElementsByTagName("impuesto_pedido");
                        XmlNodeList importe_exento_pedido = nodo.GetElementsByTagName("importe_exento_pedido");
                        XmlNodeList total_pedido = nodo.GetElementsByTagName("total_pedido");
                        XmlNodeList impuesto_int_pedido = nodo.GetElementsByTagName("impuesto_int_pedido");
                        XmlNodeList estatus_pedido = nodo.GetElementsByTagName("estatus_pedido");
                        XmlNodeList vehiculo_pedido = nodo.GetElementsByTagName("vehiculo_pedido");
                        XmlNodeList notas_pedido = nodo.GetElementsByTagName("notas_pedido");
                        XmlNodeList sucursal_pedido = nodo.GetElementsByTagName("sucursal_pedido");
                        XmlNodeList cambio_pedido = nodo.GetElementsByTagName("cambio_pedido");



                        DatosPedidos1[0, 0] = tipo_de_movimiento[i].InnerText;
                        DatosPedidos1[0, 1] = folio_pedido[i].InnerText;
                        DatosPedidos1[0, 2] = fecha_pedido[i].InnerText;
                        DatosPedidos1[0, 3] = hora_pedido[i].InnerText;
                        DatosPedidos1[0, 4] = tipo_pedido[i].InnerText;
                        DatosPedidos1[0, 5] = referencia_pedido[i].InnerText;
                        DatosPedidos1[0, 6] = docto_de_referencia[i].InnerText;
                        DatosPedidos1[0, 7] = cliente_pedido[i].InnerText;
                        DatosPedidos1[0, 8] = ruta_pedido[i].InnerText;
                        DatosPedidos1[0, 9] = vendedor_pedido[i].InnerText;
                        DatosPedidos1[0, 10] = almacen_pedido[i].InnerText;
                        DatosPedidos1[0, 11] = subtotal_pedido[i].InnerText;
                        DatosPedidos1[0, 12] = impuesto_pedido[i].InnerText;
                        DatosPedidos1[0, 13] = importe_exento_pedido[i].InnerText;
                        DatosPedidos1[0, 14] = total_pedido[i].InnerText;
                        DatosPedidos1[0, 15] = impuesto_int_pedido[i].InnerText;
                        DatosPedidos1[0, 16] = estatus_pedido[i].InnerText;
                        DatosPedidos1[0, 17] = vehiculo_pedido[i].InnerText;
                        DatosPedidos1[0, 18] = notas_pedido[i].InnerText;
                        DatosPedidos1[0, 19] = sucursal_pedido[i].InnerText;
                        DatosPedidos1[0, 20] = cambio_pedido[i].InnerText;
                        try
                        {
                            XmlNodeList folfac = nodo.GetElementsByTagName("folfac");
                            XmlNodeList venent = nodo.GetElementsByTagName("venent");
                            DatosPedidos1[0, 21] = folfac[i].InnerText;
                            DatosPedidos1[0, 22] = folfac[i].InnerText;
                        }
                        catch
                        {
                            DatosPedidos1[0, 21] = "";
                            DatosPedidos1[0, 22] = "";
                        }
                        //20160703   dijo que lo quitara la validación
                        string rutadp;
                        if (BD.consulta("SELECT COUNT(*) FROM tblRutas WHERE COD_RUTA ='" + DatosPedidos1[0, 8] + "'") == "0")
                        {
                            rutadp = ""; //No existe la ruta” + <ruta pedido>
                            escribe(3, "No existe la ruta " + DatosPedidos1[0, 8], nombre);

                        }
                        else
                        {
                            rutadp = DatosPedidos1[0, 8];
                        }

                        string vendedorN;
                        if (dtVendedores.Select("COD_VEN = '" + DatosPedidos1[0, 9] + "'").Count() == 0)
                        {

                            vendedorN = "PISO"; //“No existe el vendedor” + <vendedor> DatosNotaDeVenta1[0, 9]
                            escribe(3, "No existe el vendedor " + DatosPedidos1[0, 9], nombre);
                        }
                        else
                        {
                            vendedorN = DatosPedidos1[0, 9];
                        }
                        string almacenN;


                        if (dtCatAlmacenes.Select("COD_ALM = '" + DatosPedidos1[0, 10] + "'").Count() == 0)
                        {
                            almacenN = ConfigurationSettings.AppSettings["Almacen"].ToString();
                            escribe(3, "No existe el almacen" + DatosNotaDeVenta1[0, 3], nombre);//“No existe el almacen” + <almacen>. DatosNotaDeVenta1[0, 3]
                        }
                        else
                        {
                            almacenN = DatosPedidos1[0, 10];
                        }

                        string camion;
                        if (DatosPedidos1[0, 17] != "")
                        {
                            if (BD.consulta("SELECT COUNT(*) FROM tblCamiones WHERE NUM_CAM ='" + DatosPedidos1[0, 17] + "'") == "0")
                            {
                                camion = ""; //“No existe el almacen” + <almacen>. DatosNotaDeVenta1[0, 17]
                                escribe(3, "No existe vehículo " + DatosPedidos1[0, 17], nombre);
                            }
                            else
                            {
                                camion = DatosPedidos1[0, 17];
                            }
                        }
                        else
                        {
                            camion = DatosPedidos1[0, 17];
                        }

                        string f1 = CambioDeFecha(DatosPedidos1[0, 2]);// DatosPedidos1[0, 2].Substring(6, 4) + "-" + DatosPedidos1[0, 2].Substring(3, 2) + "-" + DatosPedidos1[0, 2].Substring(0, 2);
                        if (true)//Convert.ToDateTime(f1) > fechaTrimestre)//20151104
                        //if (Convert.ToDateTime(DatosPedidos1[0, 2].Substring(0, 10)) > fechaTrimestre)
                        {
                            if (BD.consulta("SELECT COUNT(*) FROM tblEncPedidos WHERE FOL_PED ='" + DatosPedidos1[0, 1] + "'") == "0")
                            {
                                //if (dtClientes.Select("COD_CLI = '" + DatosPedidos1[0, 7] + "'").Count() == 0)
                                //{
                                if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosPedidos1[0, 7] + "'") != "0")
                                {
                                    //Datos que no vienen en el XML:
                                    //VEN_ENT = no se graba nada
                                    //ENVIADO = 0
                                    BD.GuardaCambios("INSERT INTO tblEncPedidos(FOL_PED, FEC_PED, HORA_DOC, TIPO_PED, FOL_REF, REF_PED, COD_CLI, COD_RUTA, COD_VEN, COD_ALM, SUB_PED, IVA_PED, IMPTO_IMPTOT, TOT_PED, IMPTO_INT, STS_PED, NUM_CAM, NOTA, COD_SUCU, CAM_PED, ENVIADO, FOL_FACTURA, VEN_ENT) VALUES ('" + DatosPedidos1[0, 1] + "', '" + CambioDeFecha(DatosPedidos1[0, 2]) + "', '" + DatosPedidos1[0, 3] + "', " + Convert.ToInt16(DatosPedidos1[0, 4]) + ", '" + DatosPedidos1[0, 5] + "', '" + DatosPedidos1[0, 6] + "', '" + DatosPedidos1[0, 7] + "', '" + rutadp + "', '" + vendedorN + "', '" + almacenN + "', " + Convert.ToDecimal(DatosPedidos1[0, 11]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 12]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 13]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 14]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 15]) + ", " + Convert.ToInt16(DatosPedidos1[0, 16]) + ", '" + camion + "', '" + DatosPedidos1[0, 18] + "', '" + DatosPedidos1[0, 19] + "', " + Convert.ToDecimal(DatosPedidos1[0, 20]) + ", 0, '" + DatosPedidos1[0, 21] + "', '" + DatosPedidos1[0, 22] + "')");
                                    DatosDePedidos = true;
                                    //foliosN[cfn] = DatosNotaDeVenta[0, 1];
                                    //cfn = cfn + 1;
                                }
                                else
                                {
                                    escribe(3, "No existe el cliente " + DatosPedidos1[0, 7] + " para el pedido " + DatosPedidos1[0, 1], nombre);
                                    DatosDePedidos = false;
                                    //No existe el cliente” + <cliente pedido> + “para el pedido” + <folio pedido>
                                }
                                //}
                                //else
                                //{
                                //    BD.GuardaCambios("INSERT INTO tblEncPedidos(FOL_PED, FEC_PED, HORA_DOC, TIPO_PED, FOL_REF, REF_PED, COD_CLI, COD_RUTA, COD_VEN, COD_ALM, SUB_PED, IVA_PED, IMPTO_IMPTOT, TOT_PED, IMPTO_INT, STS_PED, NUM_CAM, NOTA, COD_SUCU, CAM_PED, ENVIADO) VALUES ('" + DatosPedidos1[0, 1] + "', '" + CambioDeFecha(DatosPedidos1[0, 2]) + "', '" + DatosPedidos1[0, 3] + "', " + Convert.ToInt16(DatosPedidos1[0, 4]) + ", '" + DatosPedidos1[0, 5] + "', '" + DatosPedidos1[0, 6] + "', '" + DatosPedidos1[0, 7] + "', '" + rutadp + "', '" + vendedorN + "', '" + almacenN + "', " + Convert.ToDecimal(DatosPedidos1[0, 11]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 12]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 13]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 14]) + ", " + Convert.ToDecimal(DatosPedidos1[0, 15]) + ", " + Convert.ToInt16(DatosPedidos1[0, 16]) + ", '" + camion + "', '" + DatosPedidos1[0, 18] + "', '" + DatosPedidos1[0, 19] + "', " + Convert.ToDecimal(DatosPedidos1[0, 20]) + ", 0)");
                                //    DatosDePedidos = true;

                                //}
                            }
                            else
                            {
                                //“Ya existe el Pedido” + FOL_PED.
                                DatosDePedidos = false;
                                //escribe(3, "Ya existe el Pedido " + DatosPedidos1[0, 1], nombre);

                            }
                        }
                        else
                        {
                            DatosDePedidos = false;
                            escribe(3, "Regristro de trimestre cerrado FEC_PED " + DatosPedidos1[0, 2], nombre);
                        }
                    }
                    #endregion
                    //}

                    if (DatosDePedidos)
                    {
                        //escribe(2, "", nombre);
                        //escribe(1, "Partidas_Pedidos", nombre);
                        //DatosPedidos2 = new string[numerodedatos, 15];
                        //for (int z = 0; z < numerodedatos; z++)
                        //{
                        lista = ((XmlElement)Pedidos[z]).GetElementsByTagName("Partidas_Pedidos");
                        #region Partidas_Pedidos
                        numerodedatos1 = lista.Count;

                        //for (int y = 0; y < numerodedatos1; y++)
                        //{
                        foreach (XmlElement nodo in lista)
                        {

                            int i = 0;

                            XmlNodeList folio = nodo.GetElementsByTagName("folio");
                            XmlNodeList articulo = nodo.GetElementsByTagName("articulo");
                            XmlNodeList cantidad = nodo.GetElementsByTagName("cantidad");
                            XmlNodeList unidad = nodo.GetElementsByTagName("unidad");
                            XmlNodeList cantidad_hh = nodo.GetElementsByTagName("cantidad_hh");
                            XmlNodeList cantidad_preventa = nodo.GetElementsByTagName("cantidad_preventa");
                            XmlNodeList cantidad_entregada = nodo.GetElementsByTagName("cantidad_entregada");
                            XmlNodeList backorder = nodo.GetElementsByTagName("backorder");
                            XmlNodeList precio_venta = nodo.GetElementsByTagName("precio_venta");
                            XmlNodeList porcentaje_descto = nodo.GetElementsByTagName("porcentaje_descto");
                            XmlNodeList precio_con_impto = nodo.GetElementsByTagName("precio_con_impto");
                            XmlNodeList porcentaje_impto1 = nodo.GetElementsByTagName("porcentaje_impto1");
                            XmlNodeList porcentaje_impto2 = nodo.GetElementsByTagName("porcentaje_impto2");
                            XmlNodeList importe_exento = nodo.GetElementsByTagName("importe_exento");
                            XmlNodeList estatus = nodo.GetElementsByTagName("estatus");
                            XmlNodeList IMP0_ART = nodo.GetElementsByTagName("IMP0_ART");
                            XmlNodeList NUM_REN = nodo.GetElementsByTagName("NUM_REN");

                            DatosPedidos2[0, 0] = folio[i].InnerText;
                            DatosPedidos2[0, 1] = articulo[i].InnerText;
                            DatosPedidos2[0, 2] = cantidad[i].InnerText;
                            DatosPedidos2[0, 3] = unidad[i].InnerText;
                            DatosPedidos2[0, 4] = cantidad_hh[i].InnerText;
                            DatosPedidos2[0, 5] = cantidad_preventa[i].InnerText;
                            DatosPedidos2[0, 6] = cantidad_entregada[i].InnerText;
                            DatosPedidos2[0, 7] = backorder[i].InnerText;
                            DatosPedidos2[0, 8] = precio_venta[i].InnerText;
                            DatosPedidos2[0, 9] = porcentaje_descto[i].InnerText;
                            DatosPedidos2[0, 10] = precio_con_impto[i].InnerText;
                            DatosPedidos2[0, 11] = porcentaje_impto1[i].InnerText;
                            DatosPedidos2[0, 12] = porcentaje_impto2[i].InnerText;
                            DatosPedidos2[0, 13] = importe_exento[i].InnerText;
                            DatosPedidos2[0, 14] = estatus[i].InnerText;

                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //    if (foliosN[r] != "" && foliosN[r] != null)
                            //    {
                            //        if (foliosN[r] == DatosPedidos2[0, 0])
                            //        {

                            string articuloN;

                            //if (dtCatArticulos.Select("COD1_ART = '" + DatosFactura1[0, 1] + "'").Count() == 0)
                            //{
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblCatArticulos WHERE COD1_ART ='" + DatosPedidos2[0, 1] + "'") == "0")
                            {
                                articuloN = "DEPURADO"; //“No existe el artículo” + <articulo> DatosPedidos2[0, 1]
                                escribe(3, "No existe el artículo " + DatosPedidos2[0, 1], nombre);
                            }
                            else
                            {
                                articuloN = DatosPedidos2[0, 1];
                            }
                            //}
                            //else
                            //{
                            articuloN = DatosPedidos2[0, 1];
                            //}

                            string unidadN;
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloN + "' AND COD_UND ='" + DatosPedidos2[0, 3] + "'") == "0")
                            {
                                unidadN = "1";//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosPedidos2[0,6]
                                escribe(3, "No coincide la equivalencia del articulo " + articuloN + " para la unidad " + DatosPedidos2[0, 6], nombre);
                            }
                            else
                            {
                                unidadN = DatosPedidos2[0, 3];
                            }

                            Decimal codigo_impto1N;
                            codigo_impto1N = Convert.ToDecimal(DatosPedidos2[0, 11]);
                            Decimal codigo_impto2N;
                            codigo_impto2N = Convert.ToDecimal(DatosPedidos2[0, 12]);

                                         //                                1       2          7         3         5        6     8       13       14        15            4       9       10        16         12         11
                            BD.GuardaCambios("INSERT INTO tblRenPedidos(FOL_PED, COD1_ART, CAN_CAR, COD_UND, CAN_HNH, CAN_PRE, CAN_ENT, CAN_BACK, PRE_ART, POR_DES, PCIO_UNI, IMP1_ART, IMP2_ART, IMPTO_IMP, STS_PED, COD_RUTA, IMP0_ART, NUM_REN) VALUES ('" + DatosPedidos2[0, 0] + "', '" + articuloN + "', " + Convert.ToDecimal(DatosPedidos2[0, 2]) + ", '" + unidadN + "' , " + Convert.ToDecimal(DatosPedidos2[0, 4]) + ", " + Convert.ToDecimal(DatosPedidos2[0, 5]) + ", " + Convert.ToDecimal(DatosPedidos2[0, 6]) + ", " + Convert.ToDecimal(DatosPedidos2[0, 7]) + ", " + Convert.ToDecimal(DatosPedidos2[0, 8]) + ", " + Convert.ToDecimal(DatosPedidos2[0, 9]) + ", " + Convert.ToDecimal(DatosPedidos2[0, 10]) + ", " + codigo_impto1N + ", " + codigo_impto2N + " , " + Convert.ToDecimal(DatosPedidos2[0, 13]) + ", " + Convert.ToInt16(DatosPedidos2[0, 14]) + " , '" + BD.consulta("SELECT COD_RUTA FROM tblencpedidos WHERE FOL_PED = '" + DatosPedidos2[0, 0] + "'") + "', " + IMP0_ART[i].InnerText + "," + NUM_REN[i].InnerText + ")");
                            //Datos que no vienen en el XML:
                            //COD_RUTA = misma que en tblEncPedidos.
                        }
                        //        }

                        //    }

                        //}
                        //}
                        #endregion
                        //}
                    }
                }
                #endregion
                //escribeArchivo(nombre);
                //-------------------------------------------------------------------------------------------------------------
                #region Estructura_del_Paquete_de_Transacciones
                XmlNodeList Operaciones_de_caja = xDoc.GetElementsByTagName("Estructura_del_Paquete_de_Transacciones");

                escribe(2, "", nombre);
                escribe(2, "", nombre);
                escribe(1, "Operaciones_de_caja", nombre);
                numerodedatos = Operaciones_de_caja.Count;
                //DatosAuxiliarCaja = new string[numerodedatos, 33];
                for (int x = 0; x < numerodedatos; x++)
                {
                    lista = ((XmlElement)Operaciones_de_caja[x]).GetElementsByTagName("Operaciones_de_caja");
                    #region Operaciones_de_caja
                    foreach (XmlElement nodo in lista)
                    {
                        int i = 0;

                        XmlNodeList tipo_de_movimiento = nodo.GetElementsByTagName("tipo_de_movimiento");
                        XmlNodeList caja = nodo.GetElementsByTagName("caja");
                        XmlNodeList turno = nodo.GetElementsByTagName("turno");
                        XmlNodeList concepto = nodo.GetElementsByTagName("concepto");
                        XmlNodeList concepto_de_caja = nodo.GetElementsByTagName("concepto_de_caja");
                        XmlNodeList concepto_general = nodo.GetElementsByTagName("concepto_general");
                        XmlNodeList folio = nodo.GetElementsByTagName("folio");
                        XmlNodeList folio_general = nodo.GetElementsByTagName("folio_general");
                        XmlNodeList referencia_general = nodo.GetElementsByTagName("referencia_general");
                        XmlNodeList fecha = nodo.GetElementsByTagName("fecha");
                        XmlNodeList hora = nodo.GetElementsByTagName("hora");
                        XmlNodeList referencia_pago = nodo.GetElementsByTagName("referencia_pago");
                        XmlNodeList referencia_adicional = nodo.GetElementsByTagName("referencia_adicional");
                        XmlNodeList cliente = nodo.GetElementsByTagName("cliente");
                        XmlNodeList forma_de_pago = nodo.GetElementsByTagName("forma_de_pago");
                        XmlNodeList moneda = nodo.GetElementsByTagName("moneda");
                        XmlNodeList tipo_de_cambio = nodo.GetElementsByTagName("tipo_de_cambio");
                        XmlNodeList importe = nodo.GetElementsByTagName("importe");
                        XmlNodeList importe_MN = nodo.GetElementsByTagName("importe_MN");
                        XmlNodeList cargo = nodo.GetElementsByTagName("cargo");
                        XmlNodeList saldo = nodo.GetElementsByTagName("saldo");
                        XmlNodeList usuarios = nodo.GetElementsByTagName("usuarios");
                        XmlNodeList autoriza = nodo.GetElementsByTagName("autoriza");
                        XmlNodeList corte_virtual = nodo.GetElementsByTagName("corte_virtual");
                        XmlNodeList corte_parcial = nodo.GetElementsByTagName("corte_parcial");
                        XmlNodeList corte_final = nodo.GetElementsByTagName("corte_final");
                        XmlNodeList contabilizado = nodo.GetElementsByTagName("contabilizado");
                        XmlNodeList importe_cambio = nodo.GetElementsByTagName("importe_cambio");
                        XmlNodeList moneda_cambio = nodo.GetElementsByTagName("moneda_cambio");
                        XmlNodeList tc_cambio = nodo.GetElementsByTagName("tc_cambio");
                        XmlNodeList notas = nodo.GetElementsByTagName("notas");
                        XmlNodeList sucursal = nodo.GetElementsByTagName("sucursal");
                        XmlNodeList cuenta_pago = nodo.GetElementsByTagName("cuenta_pago");


                        DatosAuxiliarCaja[0, 0] = tipo_de_movimiento[i].InnerText;
                        DatosAuxiliarCaja[0, 1] = caja[i].InnerText;
                        DatosAuxiliarCaja[0, 2] = turno[i].InnerText;
                        DatosAuxiliarCaja[0, 3] = concepto[i].InnerText;
                        DatosAuxiliarCaja[0, 4] = concepto_de_caja[i].InnerText;
                        DatosAuxiliarCaja[0, 5] = concepto_general[i].InnerText;
                        DatosAuxiliarCaja[0, 6] = folio[i].InnerText;
                        DatosAuxiliarCaja[0, 7] = folio_general[i].InnerText;
                        DatosAuxiliarCaja[0, 8] = referencia_general[i].InnerText;
                        DatosAuxiliarCaja[0, 9] = fecha[i].InnerText;
                        DatosAuxiliarCaja[0, 10] = hora[i].InnerText;
                        DatosAuxiliarCaja[0, 11] = referencia_pago[i].InnerText;
                        DatosAuxiliarCaja[0, 12] = referencia_adicional[i].InnerText;
                        DatosAuxiliarCaja[0, 13] = cliente[i].InnerText;
                        DatosAuxiliarCaja[0, 14] = forma_de_pago[i].InnerText;
                        DatosAuxiliarCaja[0, 15] = moneda[i].InnerText;
                        DatosAuxiliarCaja[0, 16] = tipo_de_cambio[i].InnerText;
                        DatosAuxiliarCaja[0, 17] = importe[i].InnerText;
                        DatosAuxiliarCaja[0, 18] = importe_MN[i].InnerText;
                        DatosAuxiliarCaja[0, 19] = cargo[i].InnerText;
                        DatosAuxiliarCaja[0, 20] = saldo[i].InnerText;
                        DatosAuxiliarCaja[0, 21] = usuarios[i].InnerText;
                        DatosAuxiliarCaja[0, 22] = autoriza[i].InnerText;
                        DatosAuxiliarCaja[0, 23] = corte_virtual[i].InnerText;
                        DatosAuxiliarCaja[0, 24] = corte_parcial[i].InnerText;
                        DatosAuxiliarCaja[0, 25] = corte_final[i].InnerText;
                        DatosAuxiliarCaja[0, 26] = contabilizado[i].InnerText;
                        DatosAuxiliarCaja[0, 27] = importe_cambio[i].InnerText;
                        DatosAuxiliarCaja[0, 28] = moneda_cambio[i].InnerText;
                        DatosAuxiliarCaja[0, 29] = tc_cambio[i].InnerText;
                        DatosAuxiliarCaja[0, 30] = notas[i].InnerText;
                        DatosAuxiliarCaja[0, 31] = sucursal[i].InnerText;
                        DatosAuxiliarCaja[0, 32] = cuenta_pago[i].InnerText;


                        try
                        {
                            //** 8.1.0 .. 2017-09-12
                            XmlNodeList COD_TERMINAL = nodo.GetElementsByTagName("COD_TERMINAL");
                            XmlNodeList COD_TCPROMO = nodo.GetElementsByTagName("COD_TCPROMO");
                            XmlNodeList RFC_ORDEN = nodo.GetElementsByTagName("RFC_ORDEN");
                            XmlNodeList CUENTA_ORDEN = nodo.GetElementsByTagName("CUENTA_ORDEN");
                            XmlNodeList RFC_BENEF = nodo.GetElementsByTagName("RFC_BENEF");
                            XmlNodeList CUENTA_BENEF = nodo.GetElementsByTagName("CUENTA_BENEF");
                            XmlNodeList TIPCAD_PAGO = nodo.GetElementsByTagName("TIPCAD_PAGO");
                            XmlNodeList CERT_PAGO = nodo.GetElementsByTagName("CERT_PAGO");
                            XmlNodeList CAD_PAGO = nodo.GetElementsByTagName("CAD_PAGO");
                            XmlNodeList SELLO_PAGO = nodo.GetElementsByTagName("SELLO_PAGO");
                            XmlNodeList BANCO_ORDEN = nodo.GetElementsByTagName("BANCO_ORDEN");
                            //-- 8.1.0 .. 2017-09-12

                            //** 8.1.0 .. 2017-09-12
                            DatosAuxiliarCaja[0, 33] = COD_TERMINAL[i].InnerText;
                            DatosAuxiliarCaja[0, 34] = COD_TCPROMO[i].InnerText;
                            DatosAuxiliarCaja[0, 35] = RFC_ORDEN[i].InnerText;
                            DatosAuxiliarCaja[0, 36] = CUENTA_ORDEN[i].InnerText;
                            DatosAuxiliarCaja[0, 37] = RFC_BENEF[i].InnerText;
                            DatosAuxiliarCaja[0, 38] = CUENTA_BENEF[i].InnerText;
                            DatosAuxiliarCaja[0, 39] = TIPCAD_PAGO[i].InnerText;
                            DatosAuxiliarCaja[0, 40] = CERT_PAGO[i].InnerText;
                            DatosAuxiliarCaja[0, 41] = CAD_PAGO[i].InnerText;
                            DatosAuxiliarCaja[0, 42] = SELLO_PAGO[i].InnerText;
                            DatosAuxiliarCaja[0, 43] = BANCO_ORDEN[i].InnerText;
                            //-- 8.1.0 .. 2017-09-12
                        }
                        catch
                        {
                            DatosAuxiliarCaja[0, 33] = "0";
                            DatosAuxiliarCaja[0, 34] = "0";
                            DatosAuxiliarCaja[0, 35] = "";
                            DatosAuxiliarCaja[0, 36] = "";
                            DatosAuxiliarCaja[0, 37] = "";
                            DatosAuxiliarCaja[0, 38] = "";
                            DatosAuxiliarCaja[0, 39] = "";
                            DatosAuxiliarCaja[0, 40] = "";
                            DatosAuxiliarCaja[0, 41] = "";
                            DatosAuxiliarCaja[0, 42] = "";
                            DatosAuxiliarCaja[0, 43] = "";
                        }



                        if (DatosAuxiliarCaja[0, 13] != "PUBLIC")
                        {
                            if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosNotaDeVenta[0, 4] + "'") == "0")
                            { clienteNV1 = "PUBLIC"; escribe(3, "No existe el cliente " + DatosNotaDeVenta[0, 4] + "para la NV " + DatosNotaDeVenta[0, 1], nombre); }
                            else { clienteNV1 = DatosAuxiliarCaja[0, 13]; }
                        }
                        else
                        {
                            clienteNV1 = DatosAuxiliarCaja[0, 13];
                        }


                        int formaDC;
                        if (dtFormasPago.Select("COD_FRP = " + Convert.ToInt32(DatosAuxiliarCaja[0, 14])).Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblFormasPago WHERE COD_FRP =" + Convert.ToInt32(DatosAuxiliarCaja[0, 14]) + "") == "0")
                        {
                            formaDC = 1;
                            escribe(3, "No existe la forma de pago " + DatosAuxiliarCaja[0, 14], nombre);
                        }//: “No existe la forma de pago” + <forma de pago>. DatosAuxiliarCaja[0, 14]
                        else
                        {
                            formaDC = Convert.ToInt32(DatosAuxiliarCaja[0, 14]);
                        }

                        int monedaN;
                        if (dtMoneda.Select("COD_MON = " + Convert.ToInt32(DatosAuxiliarCaja[0, 15])).Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosAuxiliarCaja[0, 15]) + "") == "0")
                        {
                            monedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[0, 15]
                            escribe(3, "No existe la moneda " + DatosAuxiliarCaja[0, 15], nombre);
                        }
                        else
                        {
                            monedaN = Convert.ToInt32(DatosAuxiliarCaja[0, 15]);
                        }

                        Int16 TCmonedaN;
                        if (dtMoneda.Select("TIP_CAM = " + Convert.ToInt32(DatosAuxiliarCaja[0, 28])).Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE TIP_CAM =" + Convert.ToDouble(DatosAuxiliarCaja[0, 28]) + "") == "0")
                        {
                            TCmonedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[0, 28]
                            escribe(3, "No existe el tipo de cambio " + DatosAuxiliarCaja[0, 28] + "de la nota de venta" + DatosAuxiliarCaja[0, 6], nombre);
                        }
                        else
                        {
                            TCmonedaN = Convert.ToInt16(DatosAuxiliarCaja[0, 28]);
                        }

                        string usuarioNV123;
                        if (dtUsuarios.Select("COD_USU = '" + DatosAuxiliarCaja[0, 21] + "'").Count() == 0)
                        {
                            usuarioNV123 = "DEPURADO";//“No existe el usuario” + <usuario>  DatosFactura[0, 21], 
                            escribe(3, "No existe el usuario " + DatosAuxiliarCaja[0, 21], nombre);
                        }
                        else
                        {
                            usuarioNV123 = DatosAuxiliarCaja[0, 21];
                        }

                        string AUTusuarioNV123="";
                        if (DatosAuxiliarCaja[0, 22].Length > 0)
                        {
                            if (dtUsuarios.Select("COD_USU = '" + DatosAuxiliarCaja[0, 22] + "'").Count() == 0)
                            //if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosAuxiliarCaja[0, 22] + "'") == "0")//20151104
                            {
                                if (usuarioNV123 != "DEPURADO")
                                    AUTusuarioNV123 = usuarioNV123;
                                else
                                    AUTusuarioNV123 = "DEPURADO"; escribe(3, "No existe el usuario " + DatosAuxiliarCaja[0, 22], nombre); 
                            }
                            else
                            {
                                AUTusuarioNV123 = DatosAuxiliarCaja[0, 22];
                            }
                        }
                        if (BD.consulta("SELECT COUNT(*) FROM tblCajas  WHERE COD_CAJ =" + Convert.ToInt32(DatosAuxiliarCaja[0, 1]) + "") == "0")
                        {
                            //“No existe la caja” + <caja>.
                            escribe(3, "No existe la caja " + DatosAuxiliarCaja[0, 1], nombre);
                            DatosAuxiliarCaja[0, 1] = "1";
                        }
                        //else
                        //{
                        if (DatosAuxiliarCaja[0, 4] == "PSER")
                        {
                            if (BD.consulta("SELECT COUNT(*) FROM tblServicios WHERE COD_SER ='" + DatosAuxiliarCaja[0, 3] + "'") == "0")
                            {
                                //“No existe el servicio de caja” + <concepto> 
                                escribe(3, "No existe el servicio " + DatosAuxiliarCaja[0, 3], nombre);
                            }
                            else
                            {
                                //** 8.1.0 .. 2017-09-09 AGREGADOS SIN VALOR .. 2017-09-12 AGREGADOS valores a los CAMPOS: FOL_COR,COD_TERMINAL,COD_TCPROMO,RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO
                                BD.GuardaCambios("INSERT INTO tblAuxCaja(COD_CAJ, TUR_CAJ, CON_CEP, COD_CON, CON_GRL, REF_DOC, FOL_GRL, REF_GRL, FEC_DOC, HORA_DOC, REF_PAG, REF_ADI, COD_CLI, COD_FRP, COD_MON, TIP_CAM, IMP_EXT, IMP_MBA, IMP_CAR, SAL_DOC, COD_USU, USU_AUT, FOL_VIR, FOL_PAR, FOL_FIN, CONTAB, IMPE_CAMBIO, MON_CAMBIO, TC_CAMBIO, NOTAS, POR_CAR, ENVIADO, COD_SUCU, CTA_PAGO,FOL_COR,COD_TERMINAL,COD_TCPROMO,RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO, BANCO_ORDEN) VALUES (" + Convert.ToInt16(DatosAuxiliarCaja[0, 1]) + ", " + Convert.ToInt16(DatosAuxiliarCaja[0, 2]) + ", '" + DatosAuxiliarCaja[0, 3] + "', '" + DatosAuxiliarCaja[0, 4] + "', '" + DatosAuxiliarCaja[0, 5] + "', '" + DatosAuxiliarCaja[0, 6] + "', '" + DatosAuxiliarCaja[0, 7] + "', '" + DatosAuxiliarCaja[0, 8] + "', '" + CambioDeFecha(DatosAuxiliarCaja[0, 9]) + "', '" + DatosAuxiliarCaja[0, 10] + "', '" + DatosAuxiliarCaja[0, 11] + "', '" + DatosAuxiliarCaja[0, 12] + "', '" + clienteNV1 + "', " + formaDC + ", " + monedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 16]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 17]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 18]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 19]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 20]) + ", '" + usuarioNV123 + "', '" + AUTusuarioNV123 + "', '" + DatosAuxiliarCaja[0, 23] + "', '" + DatosAuxiliarCaja[0, 24] + "', '" + DatosAuxiliarCaja[0, 25] + "', " + Convert.ToInt16(DatosAuxiliarCaja[0, 26]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 27]) + ", " + TCmonedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 29]) + ", '" + DatosAuxiliarCaja[0, 30] + "', 0, 0, '" + DatosAuxiliarCaja[0, 31] + "', '" + DatosAuxiliarCaja[0, 32] + "',''," + DatosAuxiliarCaja[0, 33] + "," + DatosAuxiliarCaja[0, 34] + ",'" + DatosAuxiliarCaja[0, 35] + "','" + DatosAuxiliarCaja[0, 36] + "','" + DatosAuxiliarCaja[0, 37] + "','" + DatosAuxiliarCaja[0, 38] + "','" + DatosAuxiliarCaja[0, 39] + "','" + DatosAuxiliarCaja[0, 40] + "','" + DatosAuxiliarCaja[0, 41] + "','" + DatosAuxiliarCaja[0, 42] + "','" + DatosAuxiliarCaja[0, 43] + "')");
                                //Datos que no vienen en el XML:
                                //NUM_MOV = consecutivo (incremento)
                                //POR_CAR = 0
                                //FOL_COR = no se graba nada
                                //ENVIADO = 0
                            }
                        }
                        //** 8.1.0 .. 2017-09-12 agregado else (saqué el if del else que está dentro de PSER)
                        else
                        {
                            if (DatosAuxiliarCaja[0, 4] == "RCAJ" || DatosAuxiliarCaja[0, 4] == "IEFE")
                            {
                                if (BD.consulta("SELECT COUNT(*) FROM tblConceptoIE WHERE COD_CIE ='" + DatosAuxiliarCaja[0, 3] + "'") == "0")
                                {
                                    //“No existe el concepto de caja” + <concepto>. 
                                    escribe(3, "No existe el concepto de caja " + DatosAuxiliarCaja[0, 3], nombre);
                                }
                                else
                                {
                                    //** 8.1.0 .. 2017-09-09 AGREGADOS SIN VALOR .. 2017-09-12 AGREGADOS valores a los CAMPOS: FOL_COR,COD_TERMINAL,COD_TCPROMO,RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO
                                    BD.GuardaCambios("INSERT INTO tblAuxCaja(COD_CAJ, TUR_CAJ, CON_CEP, COD_CON, CON_GRL, REF_DOC, FOL_GRL, REF_GRL, FEC_DOC, HORA_DOC, REF_PAG, REF_ADI, COD_CLI, COD_FRP, COD_MON, TIP_CAM, IMP_EXT, IMP_MBA, IMP_CAR, SAL_DOC, COD_USU, USU_AUT, FOL_VIR, FOL_PAR, FOL_FIN, CONTAB, IMPE_CAMBIO, MON_CAMBIO, TC_CAMBIO, NOTAS, POR_CAR, ENVIADO, COD_SUCU, CTA_PAGO,FOL_COR,COD_TERMINAL,COD_TCPROMO,RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO, BANCO_ORDEN) VALUES (" + Convert.ToInt16(DatosAuxiliarCaja[0, 1]) + ", " + Convert.ToInt16(DatosAuxiliarCaja[0, 2]) + ", '" + DatosAuxiliarCaja[0, 3] + "', '" + DatosAuxiliarCaja[0, 4] + "', '" + DatosAuxiliarCaja[0, 5] + "', '" + DatosAuxiliarCaja[0, 6] + "', '" + DatosAuxiliarCaja[0, 7] + "', '" + DatosAuxiliarCaja[0, 8] + "', '" + CambioDeFecha(DatosAuxiliarCaja[0, 9]) + "', '" + DatosAuxiliarCaja[0, 10] + "', '" + DatosAuxiliarCaja[0, 11] + "', '" + DatosAuxiliarCaja[0, 12] + "', '" + clienteNV1 + "', " + formaDC + ", " + monedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 16]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 17]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 18]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 19]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 20]) + ", '" + usuarioNV123 + "', '" + AUTusuarioNV123 + "', '" + DatosAuxiliarCaja[0, 23] + "', '" + DatosAuxiliarCaja[0, 24] + "', '" + DatosAuxiliarCaja[0, 25] + "', " + Convert.ToInt16(DatosAuxiliarCaja[0, 26]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 27]) + ", " + TCmonedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 29]) + ", '" + DatosAuxiliarCaja[0, 30] + "', 0, 0, '" + DatosAuxiliarCaja[0, 31] + "', '" + DatosAuxiliarCaja[0, 32] + "',''," + DatosAuxiliarCaja[0, 33] + "," + DatosAuxiliarCaja[0, 34] + ",'" + DatosAuxiliarCaja[0, 35] + "','" + DatosAuxiliarCaja[0, 36] + "','" + DatosAuxiliarCaja[0, 37] + "','" + DatosAuxiliarCaja[0, 38] + "','" + DatosAuxiliarCaja[0, 39] + "','" + DatosAuxiliarCaja[0, 40] + "','" + DatosAuxiliarCaja[0, 41] + "','" + DatosAuxiliarCaja[0, 42] + "','" + DatosAuxiliarCaja[0, 43] + "')");
                                    //Datos que no vienen en el XML:
                                    //NUM_MOV = consecutivo (incremento)
                                    //POR_CAR = 0
                                    //FOL_COR = no se graba nada
                                    //ENVIADO = 0
                                }
                            }
                            //**8.1.0 .. 2017-09-12
                            else
                            {
                                if (DatosAuxiliarCaja[0, 4] != "IVEN")
                                {
                                    if (BD.consulta("SELECT COUNT(*) FROM tblConceptos WHERE COD_CON ='" + DatosAuxiliarCaja[0, 4] + "'") == "0")
                                    {
                                        //“No existe el concepto” + <concepto>. 
                                        escribe(3, "No existe el concepto " + DatosAuxiliarCaja[0, 4], nombre);
                                    }
                                    else
                                    {
                                        //** 8.1.0 .. 2017-09-09 AGREGADOS SIN VALOR .. 2017-09-12 AGREGADOS valores a los CAMPOS: FOL_COR,COD_TERMINAL,COD_TCPROMO,RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO
                                        BD.GuardaCambios("INSERT INTO tblAuxCaja(COD_CAJ, TUR_CAJ, CON_CEP, COD_CON, CON_GRL, REF_DOC, FOL_GRL, REF_GRL, FEC_DOC, HORA_DOC, REF_PAG, REF_ADI, COD_CLI, COD_FRP, COD_MON, TIP_CAM, IMP_EXT, IMP_MBA, IMP_CAR, SAL_DOC, COD_USU, USU_AUT, FOL_VIR, FOL_PAR, FOL_FIN, CONTAB, IMPE_CAMBIO, MON_CAMBIO, TC_CAMBIO, NOTAS, POR_CAR, ENVIADO, COD_SUCU, CTA_PAGO,FOL_COR,COD_TERMINAL,COD_TCPROMO,RFC_ORDEN,CUENTA_ORDEN,RFC_BENEF,CUENTA_BENEF,TIPCAD_PAGO,CERT_PAGO,CAD_PAGO,SELLO_PAGO, BANCO_ORDEN) VALUES (" + Convert.ToInt16(DatosAuxiliarCaja[0, 1]) + ", " + Convert.ToInt16(DatosAuxiliarCaja[0, 2]) + ", '" + DatosAuxiliarCaja[0, 3] + "', '" + DatosAuxiliarCaja[0, 4] + "', '" + DatosAuxiliarCaja[0, 5] + "', '" + DatosAuxiliarCaja[0, 6] + "', '" + DatosAuxiliarCaja[0, 7] + "', '" + DatosAuxiliarCaja[0, 8] + "', '" + CambioDeFecha(DatosAuxiliarCaja[0, 9]) + "', '" + DatosAuxiliarCaja[0, 10] + "', '" + DatosAuxiliarCaja[0, 11] + "', '" + DatosAuxiliarCaja[0, 12] + "', '" + clienteNV1 + "', " + formaDC + ", " + monedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 16]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 17]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 18]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 19]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 20]) + ", '" + usuarioNV123 + "', '" + AUTusuarioNV123 + "', '" + DatosAuxiliarCaja[0, 23] + "', '" + DatosAuxiliarCaja[0, 24] + "', '" + DatosAuxiliarCaja[0, 25] + "', " + Convert.ToInt16(DatosAuxiliarCaja[0, 26]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 27]) + ", " + TCmonedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[0, 29]) + ", '" + DatosAuxiliarCaja[0, 30] + "', 0, 0, '" + DatosAuxiliarCaja[0, 31] + "', '" + DatosAuxiliarCaja[0, 32] + "',''," + DatosAuxiliarCaja[0, 33] + "," + DatosAuxiliarCaja[0, 34] + ",'" + DatosAuxiliarCaja[0, 35] + "','" + DatosAuxiliarCaja[0, 36] + "','" + DatosAuxiliarCaja[0, 37] + "','" + DatosAuxiliarCaja[0, 38] + "','" + DatosAuxiliarCaja[0, 39] + "','" + DatosAuxiliarCaja[0, 40] + "','" + DatosAuxiliarCaja[0, 41] + "','" + DatosAuxiliarCaja[0, 42] + "','" + DatosAuxiliarCaja[0, 43] + "')");
                                        //Datos que no vienen en el XML:
                                        //NUM_MOV = consecutivo (incremento)
                                        //POR_CAR = 0
                                        //FOL_COR = no se graba nada
                                        //ENVIADO = 0
                                    }
                                }
                            }
                            //--8.1.0 .. 2017-09-12
                        }
                        //-- 8.1.0 .. 2017-09-12
                        //}

                    }
                    #endregion
                }
                #endregion
                //escribeArchivo(nombre);
                //---------------------------------------------------------------------------------------------------------------
                #region Cartera
                XmlNodeList Cartera = xDoc.GetElementsByTagName("Cartera");
                escribe(2, "", nombre);
                escribe(2, "", nombre);
                escribe(1, "Cartera", nombre);
                cfn = 0;
                numerodedatos = Cartera.Count;
                //DatosCartera = new string[numerodedatos, 23];
                for (int x = 0; x < numerodedatos; x++)
                {
                    //lista = ((XmlElement)Cartera[x]).GetElementsByTagName("Cartera");
                    #region Cartera
                    //foreach (XmlElement nodo in lista)
                    //{
                        //int i = 0;

                        //XmlNodeList tipo_de_movimiento = nodo.GetElementsByTagName("tipo_de_movimiento");
                        //XmlNodeList folio = nodo.GetElementsByTagName("folio");
                        //XmlNodeList folio_general = nodo.GetElementsByTagName("folio_general");
                        //XmlNodeList concepto = nodo.GetElementsByTagName("concepto");
                        //XmlNodeList concepto_del_documento = nodo.GetElementsByTagName("concepto_del_documento");
                        //XmlNodeList concepto_general = nodo.GetElementsByTagName("concepto_general");
                        //XmlNodeList clientes = nodo.GetElementsByTagName("clientes");
                        //XmlNodeList fecha = nodo.GetElementsByTagName("fecha");
                        //XmlNodeList hora = nodo.GetElementsByTagName("hora");
                        //XmlNodeList fecha_registro = nodo.GetElementsByTagName("fecha_registro");
                        //XmlNodeList usuario = nodo.GetElementsByTagName("usuario");
                        //XmlNodeList estatus = nodo.GetElementsByTagName("estatus");
                        //XmlNodeList notas = nodo.GetElementsByTagName("notas");
                        //XmlNodeList importe = nodo.GetElementsByTagName("importe");
                        //XmlNodeList porcentaje_impuesto = nodo.GetElementsByTagName("porcentaje_impuesto");
                        //XmlNodeList importe_impuesto = nodo.GetElementsByTagName("importe_impuesto");
                        //XmlNodeList saldo = nodo.GetElementsByTagName("saldo");
                        //XmlNodeList plazo = nodo.GetElementsByTagName("plazo");
                        //XmlNodeList caja = nodo.GetElementsByTagName("caja");
                        //XmlNodeList sucursal = nodo.GetElementsByTagName("sucursal");
                        //XmlNodeList contabilizado = nodo.GetElementsByTagName("contabilizado");
                        //XmlNodeList folio_liq = nodo.GetElementsByTagName("folio_liq");
                        //XmlNodeList CONF_CFDI = nodo.GetElementsByTagName("CONF_CFDI");
                        //XmlNodeList FOL_FACT = nodo.GetElementsByTagName("FOL_FACT");

                        DatosCartera[0, 0] = Cartera[x]["tipo_de_movimiento"].InnerText; //tipo_de_movimiento[i].InnerText;
                        DatosCartera[0, 1] = Cartera[x]["folio"].InnerText;
                        DatosCartera[0, 2] = Cartera[x]["folio_general"].InnerText;
                        DatosCartera[0, 3] = Cartera[x]["concepto"].InnerText;
                        DatosCartera[0, 4] = Cartera[x]["concepto_del_documento"].InnerText;
                        DatosCartera[0, 5] = Cartera[x]["concepto_general"].InnerText;
                        DatosCartera[0, 6] = Cartera[x]["clientes"].InnerText;
                        DatosCartera[0, 7] = Cartera[x]["fecha"].InnerText;
                        DatosCartera[0, 8] = Cartera[x]["hora"].InnerText;
                        DatosCartera[0, 9] = Cartera[x]["fecha_registro"].InnerText;
                        DatosCartera[0, 10] = Cartera[x]["usuario"].InnerText;
                        DatosCartera[0, 11] = Cartera[x]["estatus"].InnerText;
                        DatosCartera[0, 12] = Cartera[x]["notas"].InnerText;
                        DatosCartera[0, 13] = Cartera[x]["importe"].InnerText;
                        DatosCartera[0, 14] = Cartera[x]["porcentaje_impuesto"].InnerText;
                        DatosCartera[0, 15] = Cartera[x]["importe_impuesto"].InnerText;
                        DatosCartera[0, 16] = Cartera[x]["saldo"].InnerText;
                        DatosCartera[0, 17] = Cartera[x]["plazo"].InnerText;
                        DatosCartera[0, 18] = Cartera[x]["caja"].InnerText;
                        DatosCartera[0, 19] = Cartera[x]["sucursal"].InnerText;
                        DatosCartera[0, 20] = Cartera[x]["contabilizado"].InnerText;
                        DatosCartera[0, 21] = Cartera[x]["folio_liq"].InnerText;//folio_liq[i].InnerText;
                        try
                        {
                            DatosCartera[0, 22] = Cartera[x]["CONF_CFDI"].InnerText;//CONF_CFDI[i].InnerText;
                        }
                        catch
                        {
                            DatosCartera[0, 22] = "";
                        }
                        try
                        {
                            DatosCartera[0, 23] = Cartera[x]["FOL_FACT"].InnerText;//FOL_FACT[i].InnerText;
                        }
                        catch
                        {
                            DatosCartera[0, 23] = "";
                        }

                        #region 2023-10-05

                        try
                        {
                            DatosCartera[0, 24] = Cartera[x]["BASE_RETIVA"].InnerText;
                        }
                        catch
                        {
                            DatosCartera[0, 24] = "0";
                        }
                        try
                        {
                            DatosCartera[0, 25] = Cartera[x]["IMP_RETIVA"].InnerText;
                        }
                        catch
                        {
                            DatosCartera[0, 25] = "0";
                        }
                        try
                        {
                            DatosCartera[0, 26] = Cartera[x]["BASE_RETISR"].InnerText;
                        }
                        catch
                        {
                            DatosCartera[0, 26] = "0";
                        }
                        try
                        {
                            DatosCartera[0, 27] = Cartera[x]["IMP_RETISR"].InnerText;
                        }
                        catch
                        {
                            DatosCartera[0, 27] = "0";
                        }


                        #endregion
                        string conceptoDF;
                        if (BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE COD_CON ='" + DatosCartera[0, 4] + "'") == "0")
                        {
                            conceptoDF = BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE TIP_MOV ='" + DatosCartera[0, 5] + "'");
                            //No existe el concepto” + <concepto del documento> + “se asignó” + TIP_MOV
                            escribe(3, "No existe el concepto " + DatosCartera[0, 4] + " se asignó " + conceptoDF, nombre);

                        }
                        else
                        {
                            conceptoDF = DatosCartera[0, 4];
                        }

                        if (DatosCartera[0, 6] != "PUBLIC")
                        {
                            if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosCartera[0, 6] + "'") == "0")
                            {
                                clienteNV1 = "PUBLIC";
                                escribe(3, "No existe el cliente " + DatosCartera[0, 6] + " para la folio " + DatosCartera[0, 1], nombre);
                            }
                            else { clienteNV1 = DatosCartera[0, 6]; }
                        }//“No existe el cliente” + <clienteNV> + “para la FACTURA + <folio NV> DatosCartera[0, 1], DatosCartera[0, 6]
                        else
                        {
                            clienteNV1 = DatosCartera[0, 6];
                        }
                        //if (dtUsuarios.Select("COD_USU = '" + DatosCartera[0, 10] + "'").Count() == 0)
                        ////if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosNotaDeVenta[0, 20] + "'") == "0")//20151104
                        //{ usuarioNV1 = "DEPURADO"; escribe(3, "No existe el usuario " + DatosCartera[0, 10], nombre); }
                        //else
                        //{
                        //    usuarioNV1 = DatosCartera[0, 10];
                        //}

                        string usuarioNV123 = "";
                        if (DatosCartera[0, 10].Length > 0)
                        {
                            if (dtUsuarios.Select("COD_USU = '" + DatosCartera[0, 10] + "'").Count() == 0)
                            {
                                usuarioNV123 = "DEPURADO";//“No existe el usuario” + <usuario>  DatosFactura[0, 21], 
                                escribe(3, "No existe el usuario " + DatosCartera[0, 10] + " para el folio " + DatosCartera[0, 1], nombre);

                            }
                            else
                            {
                                usuarioNV123 = DatosCartera[0, 10];
                            }
                        }

                        int caja321;
                        if (dtCajas.Select("COD_CAJ = " + Convert.ToInt32(DatosCartera[0, 18])).Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblCajas WHERE COD_CAJ =" + Convert.ToInt32(DatosCartera[0, 18]) + "") == "0")
                        {
                            caja321 = Convert.ToInt32(BD.consulta("SELECT COUNT(COD_CAJ) FROM tblCajas WHERE VEN_CAJ = 1 ")); //“No existe la caja de cobranza” + <caja> “se asignó 1
                            escribe(3, "No existe la caja de cobranza " + DatosCartera[0, 18] + " se asignó 1", nombre);
                        }
                        else
                        {
                            caja321 = Convert.ToInt32(DatosCartera[0, 18]);
                        }

                        //if (BD.consulta("SELECT COUNT(*) FROM tblEncCargosAbonos WHERE FOL_DOC ='" + DatosCartera[0, 1] + "'") != "0")
                        //{
                        //    //“Ya existe el movimiento en Cartera” + 3 + <folio>.DatosCartera[0, 1]
                        //    //escribe(3, "Ya existe el movimiento en Cartera " + DatosCartera[0, 3] + " " + DatosCartera[0, 1], nombre);
                        //}
                        //else
                        //{

                            if (BD.consulta("SELECT COUNT(*) FROM tblEncCargosAbonos WHERE FOL_GRL ='" + DatosCartera[0, 2] + "'") != "0")
                            {
                                string sentencia = "UPDATE tblEncCargosAbonos SET " +
                                " FOL_DOC='" + DatosCartera[0, 1] + "', COD_CON='" + DatosCartera[0, 3] + "', CON_CEP='" + conceptoDF + "', " +
                                " CON_GRL='" + DatosCartera[0, 5] + "', COD_CLI='" + clienteNV1 + "', FEC_DOC='" + CambioDeFecha(DatosCartera[0, 7]) + "', " +
                                " HORA_DOC='" + DatosCartera[0, 8] + "', FEC_REG='" + CambioDeFecha(DatosCartera[0, 9]) + "', COD_USU='" + usuarioNV123 + "', " +
                                " COD_STS=" + Convert.ToInt16(DatosCartera[0, 11]) + ", NOTA='" + DatosCartera[0, 12] + "', IMP_DOC=" + Convert.ToDecimal(DatosCartera[0, 13]) + ", " +
                                " POR_IMP=" + Convert.ToDecimal(DatosCartera[0, 14]) + ", IVA_DOC=" + Convert.ToDecimal(DatosCartera[0, 15]) + ", " +
                                " SAL_DOC=" + Convert.ToDecimal(DatosCartera[0, 16]) + ", PLA_PAG=" + Convert.ToInt64(DatosCartera[0, 17]) + ", " +
                                " COD_CAJ=" + caja321 + ", COD_SUCU='" + DatosCartera[0, 19] + "', CONTAB=" + Convert.ToInt16(DatosCartera[0, 20]) + ", " +
                                " FOL_LIQ='" + DatosCartera[0, 21] + "', ENVIADO=0, CONF_CFDI='" + DatosCartera[0, 22] + "', FOL_FACT='" + DatosCartera[0, 23] + "' "+
                                " WHERE  FOL_GRL = '" + DatosCartera[0, 2] + "';";
                                BD.GuardaCambios(sentencia);
                                //“Ya existe el movimiento en Cartera” + 3 + <folio>.DatosCartera[0, 2]
                                //escribe(3, "Ya existe el movimiento en Cartera " + DatosCartera[0, 3] + " " + DatosCartera[0, 2], nombre);
                                DatosDeCartera = false;
                            }
                            else
                            {
                                //Se agrego el campo IMP_RETISR, BASE_RETIVA, IMP_RETIVA, BASE_RETISR con su default 20231005
                                string sentencia = "INSERT INTO tblEncCargosAbonos(FOL_DOC, FOL_GRL, COD_CON, CON_CEP, CON_GRL, COD_CLI, FEC_DOC, HORA_DOC, FEC_REG, COD_USU, COD_STS, NOTA, IMP_DOC, POR_IMP, IVA_DOC, SAL_DOC, PLA_PAG, COD_CAJ, COD_SUCU, CONTAB, FOL_LIQ, ENVIADO, CONF_CFDI, FOL_FACT, BASE_RETIVA, IMP_RETIVA, BASE_RETISR, IMP_RETISR) VALUES ('" + DatosCartera[0, 1] + "', '" + DatosCartera[0, 2] + "', '" + DatosCartera[0, 3] + "', '" + conceptoDF + "', '" + DatosCartera[0, 5] + "', '" + clienteNV1 + "', '" + CambioDeFecha(DatosCartera[0, 7]) + "', '" + DatosCartera[0, 8] + "', '" + CambioDeFecha(DatosCartera[0, 9]) + "', '" + usuarioNV123 + "', " + Convert.ToInt16(DatosCartera[0, 11]) + ", '" + DatosCartera[0, 12] + "', " + Convert.ToDecimal(DatosCartera[0, 13]) + ", " + Convert.ToDecimal(DatosCartera[0, 14]) + ", " + Convert.ToDecimal(DatosCartera[0, 15]) + ", " + Convert.ToDecimal(DatosCartera[0, 16]) + ", " + Convert.ToInt64(DatosCartera[0, 17]) + ", " + caja321 + ", '" + DatosCartera[0, 19] + "', " + Convert.ToInt16(DatosCartera[0, 20]) + ", '" + DatosCartera[0, 21] + "', 0, '" + DatosCartera[0, 22] + "', '" +
                                    DatosCartera[0, 23] + "', " + DatosCartera[0, 24] + ", " + DatosCartera[0, 25] + ", " + DatosCartera[0, 26] + ", " + DatosCartera[0, 27] + ")";
                                //MessageBox.Show(sentencia + Environment.NewLine + clienteNV1);
                                BD.GuardaCambios(sentencia);
                                if (clienteNV1 != "PUBLIC")
                                {

                                    decimal SaldoTotal;
                                    string datototal;

                                    datototal = BD.consulta("SELECT SAL_CLI FROM tblCatClientes WHERE COD_CLI = '" + clienteNV1 + "'");


                                    SaldoTotal = Convert.ToDecimal(datototal);

                                    if (DatosCartera[0, 5] == "CCLI")
                                    {
                                        SaldoTotal = SaldoTotal + Convert.ToDecimal(DatosCartera[0, 13]);
                                    }
                                    else
                                    {
                                        if (DatosCartera[0, 5] == "ACLI")
                                        {
                                            SaldoTotal = SaldoTotal - Convert.ToDecimal(DatosCartera[0, 13]);
                                        }
                                    }

                                    BD.GuardaCambios("UPDATE tblCatClientes SET SAL_CLI = " + SaldoTotal + " WHERE COD_CLI = '" + clienteNV1 + "'");
                                }

                                DatosDeCartera = true;
                                //Datos que no vienen en el XML:
                                //NUM_MOV = consecutivo (incremento)
                                //ENVIADO = 0
                                //foliosN[cfn] = DatosCartera[0, 1];
                                //cfn = cfn + 1;
                            }

                        //}
                        //x = x + 1;
                    //}
                    #endregion
                    //}

                    //--------------------------------------------------------------------------------------------
                    //int folioenc;
                    //folioenc = 0;
                    //for (int x = 0; x < numerodedatos; x++)
                    //{
                    if (DatosDeCartera)
                    {
                        lista = ((XmlElement)Cartera[x]).GetElementsByTagName("Documentos_Afectados");
                        #region Documentos_Afectados
                        //numerodedatos1 = lista.Count;
                        //for (int y = 0; y < numerodedatos1; y++)
                        //{


                        foreach (XmlElement nodo in lista)
                        {
                            int i = 0;

                            XmlNodeList folio_carter = nodo.GetElementsByTagName("folio_carter");
                            XmlNodeList folio_documento = nodo.GetElementsByTagName("folio_documento");
                            XmlNodeList folio_general = nodo.GetElementsByTagName("folio_general");
                            XmlNodeList concepto = nodo.GetElementsByTagName("concepto");
                            XmlNodeList concepto_documento = nodo.GetElementsByTagName("concepto_documento");
                            XmlNodeList concepto_general = nodo.GetElementsByTagName("concepto_general");
                            XmlNodeList importe_aplicado = nodo.GetElementsByTagName("importe_aplicado");
                            XmlNodeList saldo_del_renglón = nodo.GetElementsByTagName("saldo_del_renglón");
                            XmlNodeList total_del_documento = nodo.GetElementsByTagName("total_del_documento");
                            XmlNodeList estatus = nodo.GetElementsByTagName("estatus");

                            DatosCartera1[0, 0] = folio_carter[i].InnerText;
                            DatosCartera1[0, 1] = folio_documento[i].InnerText;
                            DatosCartera1[0, 2] = folio_general[i].InnerText;
                            DatosCartera1[0, 3] = concepto[i].InnerText;
                            DatosCartera1[0, 4] = concepto_documento[i].InnerText;
                            DatosCartera1[0, 5] = concepto_general[i].InnerText;
                            DatosCartera1[0, 6] = importe_aplicado[i].InnerText;
                            DatosCartera1[0, 7] = saldo_del_renglón[i].InnerText;
                            DatosCartera1[0, 8] = total_del_documento[i].InnerText;
                            DatosCartera1[0, 9] = estatus[i].InnerText;


                            //for (int r = 0; r < cfn; r++)
                            //{
                            //if (foliosN[folioenc] != "" && foliosN[folioenc] != null)
                            //{
                            //    if (foliosN[folioenc] == DatosCartera1[0, 0])
                            //    {
                            string xyz;
                            Int64 qwe;
                            qwe = Convert.ToInt64(BD.BuscaRegistroConVariasCondiciones("SELECT NUM_MOV FROM tblEncCargosAbonos WHERE  FOL_DOC ='" + DatosCartera1[0, 0] + "'"));
                            if (BD.consulta("SELECT COUNT(*) FROM tblRenCargosAbonos WHERE FOL_REF ='" + DatosCartera1[0, 1] + "' AND FOL_GRL ='" + DatosCartera1[0, 2] + "'") == "0")
                            {
                                xyz = "INSERT INTO tblRenCargosAbonos(FOL_DOC, FOL_REF, FOL_GRL, COD_CON, CON_CEP, CON_GRL, IMP_DOC, SAL_DOC, TOT_DOC, COD_STS, FEC_DOC, COD_CLI, NUM_MOV) VALUES ('" + DatosCartera1[0, 0] + "', '" + DatosCartera1[0, 1] + "', '" + DatosCartera1[0, 2] + "', '" + DatosCartera1[0, 3] + "', '" + DatosCartera1[0, 4] + "', '" + DatosCartera1[0, 5] + "', " + Convert.ToDecimal(DatosCartera1[0, 6]) + ", " + Convert.ToDecimal(DatosCartera1[0, 7]) + ", " + Convert.ToDecimal(DatosCartera1[0, 8]) + ", " + Convert.ToInt16(DatosCartera1[0, 9]) + ", '" + CambioDeFecha(DatosCartera[0, 7]) + "', '" + DatosCartera[0, 6] + "', " + qwe + ")";
                                BD.GuardaCambios(xyz);

                                //Saldos
                                XmlNodeList listaN2 = nodo.GetElementsByTagName("Saldo_Documento");
                                foreach (XmlElement nodo3 in listaN2)
                                {
                                    XmlNodeList SAL_DOC = nodo3.GetElementsByTagName("SAL_DOC");
                                    //Sustituye Saldo_Documento(SAL_DOC) en el encablezado del documento afectado 
                                    //tblEncCargosAbono WHERE FOL_DOC = FOL_REF
                                    string sentencia = " UPDATE tblEncCargosAbonos SET SAL_DOC = " + SAL_DOC[0].InnerText + "  WHERE FOL_DOC = '" + folio_documento[0].InnerText + "'; ";
                                    BD.FunicionEjecucion(sentencia);
                                    sentencia = " UPDATE tblRenCargosAbonos SET SAL_DOC = " + SAL_DOC[0].InnerText + "  WHERE FOL_DOC = '" + folio_documento[0].InnerText + "'; ";
                                    BD.FunicionEjecucion(sentencia);
                                }



                            }
                            //y = y + 1;
                            //Datos que no vienen en el XML:
                            //FEC_DOC = al de encabezado que le corresponde
                            //COD CLI = al de encabezado que le corresponde
                            //NUM_MOV = el del encabezado que le corresponde
                            //    }
                            //} y = y + 1;

                            //}

                        } //folioenc = folioenc + 1;
                        //}
                        #endregion
                    }
                }
                #endregion
                //escribeArchivo(nombre);
                //--------------------------------------------------------------------------------------------------------------
                #region Entradas_y_Salida

                ////MessageBox.Show("almacenes");
                //escribe(2, "", nombre);
                //escribe(2, "", nombre);
                escribe(1, "Entradas_y_Salida", nombre);
                XmlNodeList Entradas_y_Salida = xDoc.GetElementsByTagName("Entradas_y_Salida");

                numerodedatos = Entradas_y_Salida.Count;
                //datos1 = new string[numerodedatos, 23];
                cfn = 0;
                for (int z = 0; z < numerodedatos; z++)
                {
                    lista = ((XmlElement)Entradas_y_Salida[z]).GetElementsByTagName("Datos_Generales_Almacen");
                    #region Datos_Generales_Almacen
                    foreach (XmlElement nodo in lista)
                    {

                        int i = 0;

                        XmlNodeList tipo_de_movimiento =
                        nodo.GetElementsByTagName("tipo_de_movimiento");

                        XmlNodeList folio =
                        nodo.GetElementsByTagName("folio");

                        XmlNodeList folio_general =
                        nodo.GetElementsByTagName("folio_general");

                        XmlNodeList almacen =
                        nodo.GetElementsByTagName("almacen");

                        XmlNodeList concepto =
                        nodo.GetElementsByTagName("concepto");

                        XmlNodeList concepto_almacen =
                        nodo.GetElementsByTagName("concepto_almacen");

                        XmlNodeList concepto_general =
                        nodo.GetElementsByTagName("concepto_general");

                        XmlNodeList fecha =
                        nodo.GetElementsByTagName("fecha");

                        XmlNodeList fecha_registro =
                        nodo.GetElementsByTagName("fecha_registro");

                        XmlNodeList hora =
                        nodo.GetElementsByTagName("hora");

                        XmlNodeList moneda =
                        nodo.GetElementsByTagName("moneda");

                        XmlNodeList tipo_de_cambio =
                        nodo.GetElementsByTagName("tipo_de_cambio");

                        XmlNodeList renglones =
                        nodo.GetElementsByTagName("renglones");

                        XmlNodeList cantidad_total =
                        nodo.GetElementsByTagName("cantidad_total");

                        XmlNodeList costo_total =
                        nodo.GetElementsByTagName("costo_total");

                        XmlNodeList estatus =
                        nodo.GetElementsByTagName("estatus");

                        XmlNodeList almacen_de_referencia =
                        nodo.GetElementsByTagName("almacen_de_referencia");

                        XmlNodeList referencia_adicional =
                        nodo.GetElementsByTagName("referencia_adicional");

                        XmlNodeList referencia_traspaso =
                        nodo.GetElementsByTagName("referencia_traspaso");

                        XmlNodeList usuario =
                        nodo.GetElementsByTagName("usuario");

                        XmlNodeList autoriza =
                        nodo.GetElementsByTagName("autoriza");

                        XmlNodeList sucursal =
                        nodo.GetElementsByTagName("sucursal");

                        XmlNodeList contabilizado =
                        nodo.GetElementsByTagName("contabilizado");

                        datos1[0, 0] = tipo_de_movimiento[i].InnerText;
                        datos1[0, 1] = folio[i].InnerText;
                        datos1[0, 2] = folio_general[i].InnerText;
                        datos1[0, 3] = almacen[i].InnerText;
                        datos1[0, 4] = concepto[i].InnerText;
                        datos1[0, 5] = concepto_almacen[i].InnerText;
                        datos1[0, 6] = concepto_general[i].InnerText;
                        datos1[0, 7] = fecha[i].InnerText;
                        datos1[0, 8] = fecha_registro[i].InnerText;
                        datos1[0, 9] = hora[i].InnerText;
                        datos1[0, 10] = moneda[i].InnerText;
                        datos1[0, 11] = tipo_de_cambio[i].InnerText;
                        datos1[0, 12] = renglones[i].InnerText;
                        datos1[0, 13] = cantidad_total[i].InnerText;
                        datos1[0, 14] = costo_total[i].InnerText;
                        datos1[0, 15] = estatus[i].InnerText;
                        datos1[0, 16] = almacen_de_referencia[i].InnerText;
                        datos1[0, 17] = referencia_adicional[i].InnerText;
                        datos1[0, 18] = referencia_traspaso[i].InnerText;
                        datos1[0, 19] = usuario[i].InnerText;
                        datos1[0, 20] = autoriza[i].InnerText;
                        datos1[0, 21] = sucursal[i].InnerText;
                        datos1[0, 22] = contabilizado[i].InnerText;

                        string conceptoDF;
                        if (BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE COD_CON ='" + datos1[0, 5] + "'") == "0")
                        {
                            conceptoDF = BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE TIP_MOV ='" + datos1[0, 6] + "'");
                            //No existe el concepto” + <concepto del documento> + “se asignó” + TIP_MOV
                            escribe(3, "No existe el concepto " + datos1[0, 5] + " se asignó " + conceptoDF, nombre);

                        }
                        else
                        {
                            conceptoDF = datos1[0, 5];
                        }

                        int monedaN;
                        if (dtMoneda.Select("COD_MON = " + Convert.ToInt32(datos1[0, 10])).Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(datos1[0, 10]) + "") == "0")
                        {
                            monedaN = 1; //“No existe la moneda” + <moneda> DatosNotaDeVenta1[0, 10]
                            escribe(3, "No existe la moneda " + datos1[0, 10], nombre);

                        }
                        else
                        {
                            monedaN = Convert.ToInt32(datos1[0, 10]);
                        }

                        string codigoformaNV;
                        if (dtUsuarios.Select("COD_USU = '" + datos1[0, 19] + "'").Count() == 0)
                        //if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + datos1[0, 19] + "'") == "0")
                        {
                            codigoformaNV = "DEPURADO"; //“No existe la forma de pago” + <código forma>.
                            escribe(3, "No existe el usuario " + datos1[0, 19], nombre);
                        }
                        else
                        {
                            codigoformaNV = datos1[0, 19];
                        }

                        string f1 = CambioDeFecha(datos1[0, 7]);// datos1[0, 7].Substring(6, 4) + "-" + datos1[0, 7].Substring(3, 2) + "-" + datos1[0, 7].Substring(0, 2);
                        if (true)//Convert.ToDateTime(f1) > fechaTrimestre)//20151104
                        {

                            if (BD.consulta("SELECT COUNT(*) FROM tblGralAlmacen WHERE REF_MOV ='" + datos1[0, 1] + "'") != "0")
                            {
                                //“Ya existe el movimiento de almacén” + <concepto> + REF_MOV.DatosCartera[x, 1]
                                //escribe(3, "Ya existe el movimiento de almacén " + datos1[0, 1], nombre);
                                DatosDeAlmacen = false;

                            }
                            else
                            {
                                if (BD.consulta("SELECT COUNT(*) FROM tblGralAlmacen WHERE FOL_GRL ='" + datos1[0, 2] + "'") != "0")
                                {
                                    //“Ya existe el movimiento de almacén” + <concepto> + REF_MOV
                                    //escribe(3, "Ya existe el movimiento de almacén " + datos1[0, 2], nombre);
                                    DatosDeAlmacen = false;

                                }
                                else
                                {
                                    if (datos1[0, 3] != "")
                                    {
                                        if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + datos1[0, 3] + "'") == "0")
                                        {
                                            //“No existe el almacen” + <almacen> + “no se agregó el movimiento”. Aplica lo mismo para <almacen de referencia>
                                            escribe(3, "No existe el almacen " + datos1[0, 3], nombre);
                                            DatosDeAlmacen = false;
                                        }
                                        else
                                        {
                                            BD.GuardaCambios("INSERT INTO tblGralAlmacen(REF_MOV, FOL_GRL, COD_ALM, CON_CEP, COD_CON, CON_GRL, FEC_MOV, FEC_REG, HORA_MOV, COD_MON, TIP_CAM, NUM_REN, SUM_CAN, COS_TOT, COD_STS, ALM_REF, REF_ADI, REF_TRA, COD_USU, USU_AUT, SUC_REF, CONTAB, ENVIADO ) VALUES ('" + datos1[0, 1] + "', '" + datos1[0, 2] + "', '" + datos1[0, 3] + "', '" + datos1[0, 4] + "', '" + conceptoDF + "', '" + datos1[0, 6] + "', '" + CambioDeFecha(datos1[0, 7]) + "', '" + CambioDeFecha(datos1[0, 8]) + "', '" + datos1[0, 9] + "', " + monedaN + ", " + Convert.ToDecimal(datos1[0, 11]) + ", " + Convert.ToInt64(datos1[0, 12]) + ", " + verificaLongitud(datos1[0, 13]) + ", " + verificaLongitud(datos1[0, 14]) + ", " + Convert.ToInt32(datos1[0, 15]) + ", '" + datos1[0, 16] + "', '" + datos1[0, 17] + "', '" + datos1[0, 18] + "', '" + codigoformaNV + "', '" + datos1[0, 20] + "', '" + datos1[0, 21] + "', " + Convert.ToInt32(datos1[0, 22]) + ", 0)");
                                            //Datos que no vienen en el XML:
                                            //NUM_MOV = consecutivo (incremento)
                                            //ENVIADO = 0
                                            FOCATI[0, 0] = datos1[0, 1];//folio
                                            FOCATI[0, 1] = datos1[0, 3];//Codigo Almacen
                                            FOCATI[0, 2] = datos1[0, 6];//Tipo de Movimiento nuevo
                                            //cfn = cfn + 1;
                                            DatosDeAlmacen = true;
                                        }
                                    }
                                    else
                                    {
                                        BD.GuardaCambios("INSERT INTO tblGralAlmacen(REF_MOV, FOL_GRL, COD_ALM, CON_CEP, COD_CON, CON_GRL, FEC_MOV, FEC_REG, HORA_MOV, COD_MON, TIP_CAM, NUM_REN, SUM_CAN, COS_TOT, COD_STS, ALM_REF, REF_ADI, REF_TRA, COD_USU, USU_AUT, SUC_REF, CONTAB, ENVIADO ) VALUES ('" + datos1[0, 1] + "', '" + datos1[0, 2] + "', '" + datos1[0, 3] + "', '" + datos1[0, 4] + "', '" + conceptoDF + "', '" + datos1[0, 6] + "', '" + CambioDeFecha(datos1[0, 7]) + "', '" + CambioDeFecha(datos1[0, 8]) + "', '" + datos1[0, 9] + "', " + monedaN + ", " + Convert.ToDecimal(datos1[0, 11]) + ", " + Convert.ToInt64(datos1[0, 12]) + ", " + verificaLongitud(datos1[0, 13]) + ", " + verificaLongitud(datos1[0, 14]) + ", " + Convert.ToInt32(datos1[0, 15]) + ", '" + datos1[0, 16] + "', '" + datos1[0, 17] + "', '" + datos1[0, 18] + "', '" + codigoformaNV + "', '" + datos1[0, 20] + "', '" + datos1[0, 21] + "', " + Convert.ToInt32(datos1[0, 22]) + ", 0)");

                                        //Datos que no vienen en el XML:
                                        //NUM_MOV = consecutivo (incremento)
                                        //ENVIADO = 0
                                        FOCATI[0, 0] = datos1[0, 1];//folio
                                        FOCATI[0, 1] = datos1[0, 3];//Codigo Almacen
                                        FOCATI[0, 2] = datos1[0, 6];//Tipo de Movimiento
                                        //cfn = cfn + 1;
                                        DatosDeAlmacen = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            escribe(3, "Regristro de trimestre cerrado FEC_MOV " + CambioDeFecha(datos1[0, 7]), nombre);
                        }
                    }
                    #endregion
                    //}
                    //------------------------------------------------------------------------------------------

                    if (DatosDeAlmacen)
                    {
                        //escribe(2, "", nombre);
                        //escribe(1, "Renglones_Almacen", nombre);
                        int folioenc = 0;
                        bool banderaRenglones = false;
                        //for (int z = 0; z < numerodedatos; z++)
                        //{
                        lista = ((XmlElement)Entradas_y_Salida[z]).GetElementsByTagName("Renglones_Almacen");
                        #region Renglones_Almacen
                        numerodedatos1 = lista.Count;

                        //for (int y = 0; y < numerodedatos1; y++)
                        //{
                        foreach (XmlElement nodo in lista)
                        {

                            int i = 0;

                            XmlNodeList folio =
                            nodo.GetElementsByTagName("folio");

                            XmlNodeList almacen =
                            nodo.GetElementsByTagName("almacen");

                            XmlNodeList almacen_de_referencia =
                            nodo.GetElementsByTagName("almacen_de_referencia");

                            XmlNodeList articulo =
                            nodo.GetElementsByTagName("articulo");

                            XmlNodeList cantidad =
                            nodo.GetElementsByTagName("cantidad");

                            XmlNodeList unidad =
                            nodo.GetElementsByTagName("unidad");

                            XmlNodeList equivalencia =
                            nodo.GetElementsByTagName("equivalencia");

                            XmlNodeList costo_unitario =
                            nodo.GetElementsByTagName("costo_unitario");

                            XmlNodeList lote =
                            nodo.GetElementsByTagName("lote");

                            XmlNodeList fecha_de_caducidad =
                            nodo.GetElementsByTagName("fecha_de_caducidad");

                            XmlNodeList usuario =
                            nodo.GetElementsByTagName("usuario");

                            XmlNodeList tipo_de_cambio =
                            nodo.GetElementsByTagName("tipo_de_cambio");


                            XmlNodeList NUM_PEDIM =
                            nodo.GetElementsByTagName("NUM_PEDIM");


                            XmlNodeList FEC_PEDIM =
                            nodo.GetElementsByTagName("FEC_PEDIM");


                            datos[0, 0] = folio[i].InnerText;
                            datos[0, 1] = almacen[i].InnerText;
                            datos[0, 2] = almacen_de_referencia[i].InnerText;
                            datos[0, 3] = articulo[i].InnerText;
                            datos[0, 4] = cantidad[i].InnerText;
                            datos[0, 5] = unidad[i].InnerText;
                            datos[0, 6] = equivalencia[i].InnerText;
                            datos[0, 7] = costo_unitario[i].InnerText;
                            datos[0, 8] = lote[i].InnerText;
                            datos[0, 9] = fecha_de_caducidad[i].InnerText;
                            datos[0, 10] = usuario[i].InnerText;
                            datos[0, 11] = tipo_de_cambio[i].InnerText;


                            //for (int r = 0; foliosN.Length > r; r++)
                            //{
                            //if (FOCATI[folioenc, 0] != "" && FOCATI[folioenc, 0] != null)
                            //{
                            //if (FOCATI[folioenc, 0] == datos[0, 0])
                            //{
                            banderaRenglones = true;
                            string codigoAlmacen;
                            if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + datos[0, 1] + "'") == "0")
                            {
                                codigoAlmacen = FOCATI[0, 1];
                                //“No existe el almacen” + <almacen> + “no se agregó el movimiento”                            escribe(1, "Ya existe el movimiento de almacén " + datos1[0, 2], nombre);
                                escribe(3, "No existe el almacen " + datos[0, 1], nombre);

                            }
                            else
                            {
                                codigoAlmacen = datos[0, 1];
                            }

                            string codigoformaNV1 = "";

                            if (datos[0, 2].Length > 0)//corregido 16-abr-2013 .. no existía validación
                            {
                                if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + datos[0, 2] + "'") == "0") //corregido 16-abr-2013 ... La validación tenía != en lugar de ==
                                {
                                    codigoformaNV1 = BD.consulta("SELECT COD_ALM FROM tblGralAlmacen WHERE REF_MOV ='" + datos[0, 0] + "'");
                                    //“No existe el almacen” + <almacen> + “no se agregó el movimiento”
                                    escribe(3, "No existe el almacen " + datos[0, 2], nombre);
                                }
                                else
                                {
                                    codigoformaNV1 = datos[0, 2];
                                }
                            }

                            string unidadN;
                            if (BD.consulta("SELECT COUNT(COD1_ART) FROM tblUndCosPreArt WHERE COD1_ART ='" + datos[0, 3] + "' AND COD_UND ='" + datos[0, 5] + "'") == "0")
                            {
                                unidadN = "1";//: “No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad>
                                escribe(3, "No coincide la equivalencia del articulo " + datos[0, 3], nombre);

                            }
                            else
                            {
                                unidadN = datos[0, 5];
                            }


                            string codigoformaNV123;

                            if (dtUsuarios.Select("COD_USU = '" + datos[0, 10] + "'").Count() == 0)
                            {
                                codigoformaNV123 = "DEPURADO"; //“No existe la forma de pago” + <código forma>.
                                escribe(3, "No existe el usuario " + datos[0, 10], nombre);
                            }
                            else
                            {
                                codigoformaNV123 = datos[0, 10];
                            }
                            string abc = "", abc2 = "", abc3 = "";
                            DataTable datoss = BD.ObtieneDatosParaDataTableH1("SELECT NUM_MOV, FOL_GRL, SUC_REF  FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[0, 0] + "'");
                            if (datoss.Rows.Count > 0)
                            {
                                abc = datoss.Rows[0]["NUM_MOV"].ToString();
                                abc2 = datoss.Rows[0]["FOL_GRL"].ToString();
                                abc3 = datoss.Rows[0]["SUC_REF"].ToString();
                            }
                            else
                            {
                                abc = ""; abc2 = ""; abc3 = "";
                            }
                            //abc = AC_Consultas.BuscaRegistroConVariasCondicionesH1("SELECT NUM_MOV FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[0, 0] + "'", AC_General.conexionPVH1);
                            //abc2 = AC_Consultas.BuscaRegistroConVariasCondicionesH1("SELECT FOL_GRL  FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[0, 0] + "'", AC_General.conexionPVH1);
                            //abc3 = AC_Consultas.BuscaRegistroConVariasCondicionesH1("SELECT SUC_REF FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[0, 0] + "'", AC_General.conexionPVH1);
                            Int64 ab1;
                            decimal cEqvBase;
                            string cnn;
                            cEqvBase = 0;
                            if (abc.Length > 0)
                                ab1 = Convert.ToInt64(abc);
                            else
                                ab1 = 0;
                            BD.GuardaCambios("INSERT INTO tblRenAlmacen(REF_MOV, COD_ALM, ALM_REF, COD1_ART, CAN_REN, COD_UND, EQV_UND, COS_UNI, NUM_LOT, FEC_CAD, COD_USU, TC_ART, NUM_MOV, FOL_GRL, SUC_REF, NUM_PEDIM, FEC_PEDIM) VALUES ('" + datos[0, 0] + "', '" + codigoAlmacen + "', '" + codigoformaNV1 + "', '" + datos[0, 3] + "', " + verificaLongitud(datos[0, 4]) + ", '" + unidadN + "', " + Convert.ToDecimal(datos[0, 6]) + ", " + verificaLongitud(datos[0, 7]) + ", '" + datos[0, 8] + "', '" + CambioDeFecha(datos[0, 9]) + "',  '" + codigoformaNV123 + "', " + Convert.ToDecimal(datos[0, 11]) + ", " + ab1 + ", '" + abc2 + "', '" + abc3 + "', '" + NUM_PEDIM[i].InnerText + "', '" + FEC_PEDIM[i].InnerText + "')");

                            string almacenOficial;
                            decimal cantidadArticulo;

                            cantidadArticulo = Convert.ToDecimal(datos[0, 4]) * Convert.ToDecimal(datos[0, 6]);

                            almacenOficial = FOCATI[0, 1];

                            if (almacenOficial == "" || almacenOficial == null || almacenOficial != codigoAlmacen)
                            {
                                almacenOficial = codigoAlmacen;
                            }

                            string guardaCatArticulo = "", guardaExoPorAlmcen = "";


                            cCosPEPS = 0;
                            cnn = cnnxion;// AC_General.conexionPV.ConnectionString.ToString();
                            switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
                            {
                                case "MySQL": cnn = ConfigurationSettings.AppSettings["connStringMySQL"].ToString(); break;
                                case "MSSQL": cnn = ConfigurationSettings.AppSettings["connStringMSSQL"].ToString(); break;
                            }
                            cEqvBase = Convert.ToDecimal(datos[0, 4]) * Convert.ToDecimal(datos[0, 6]);

                            if (FOCATI[0, 2] == "EALM")
                            {
                                //objCostos.GrabarCapaDeCostos(ref cnn, datos[0, 0], "'" + DateTime.Now.ToString("yyyyMMdd") + "'", "EALM", datos[0, 3], Convert.ToDecimal(datos[0, 4]), unidadN, Convert.ToDecimal(datos[0, 6]), Convert.ToDecimal(datos[0, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[0, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'");
                                GrabarCapaDeCostos(datos[0, 0], "'" + DateTime.Now.ToString("yyyyMMdd") + "'", "EALM", datos[0, 3], Convert.ToDecimal(datos[0, 4]), unidadN, Convert.ToDecimal(datos[0, 6]), Convert.ToDecimal(datos[0, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[0, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'", NUM_PEDIM[i].InnerText,  FEC_PEDIM[i].InnerText);

                                guardaCatArticulo = "UPDATE tblCatArticulos Set EXI_ACT = EXI_ACT + " + cantidadArticulo + " WHERE COD1_ART = '" + datos[0, 3] + "'";
                                guardaExoPorAlmcen = "UPDATE tblExiPorAlmacen Set EXI_ALM = EXI_ALM + " + cantidadArticulo + " WHERE COD1_ART = '" + datos[0, 3] + "' AND COD_ALM = '" + almacenOficial + "'";
                                BD.GuardaCambios(guardaCatArticulo);
                                BD.GuardaCambios(guardaExoPorAlmcen);
                                //objCostos.CalCostoPromedioNET(datos[0, 3], cnn, "'" + DateTime.Now.ToString("yyyyMMdd") + "'");
                                CalCostoPromedioNET(datos[0, 3], cnn, "'" + DateTime.Now.ToString("yyyyMMdd") + "'");
                            }
                            else
                            {
                                if (FOCATI[0, 2] == "SALM")
                                {
                                    //20150325 se agrego la validaion de longitud a precio unitario datos[0, 7]
                                    //objCostos.GrabarCapaDeCostos(ref cnn, datos[0, 0], "'"+DateTime.Now.ToString("yyyyMMdd")+"'", "SALM", datos[0, 3], Convert.ToDecimal(datos[0, 4]), unidadN, Convert.ToDecimal(datos[0, 6]), Convert.ToDecimal(datos[0, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[0, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'");
                                    //objCostos.GrabarCapaDeCostos(ref cnn, datos[0, 0], "'" + DateTime.Now.ToString("yyyyMMdd") + "'", "SALM", datos[0, 3], Convert.ToDecimal(datos[0, 4]), unidadN, Convert.ToDecimal(datos[0, 6]), verificaLongitud(datos[0, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[0, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'");
                                    GrabarCapaDeCostos(datos[0, 0], "'" + DateTime.Now.ToString("yyyyMMdd") + "'", "SALM", datos[0, 3], Convert.ToDecimal(datos[0, 4]), unidadN, Convert.ToDecimal(datos[0, 6]), verificaLongitud(datos[0, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[0, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'", NUM_PEDIM[i].InnerText,  FEC_PEDIM[i].InnerText);
                                    SalidaPromedioNET(codigoAlmacen, datos[0, 0], cCosPEPS, datos[0, 3]);
                                    //objCostos.SalidaPromedioNET(codigoAlmacen, datos[0, 0], ref cCosPEPS, datos[0, 3], cnn);

                                    guardaCatArticulo = "UPDATE tblCatArticulos Set EXI_ACT = EXI_ACT - " + cantidadArticulo + " WHERE COD1_ART = '" + datos[0, 3] + "'";
                                    guardaExoPorAlmcen = "UPDATE tblExiPorAlmacen Set EXI_ALM = EXI_ALM - " + cantidadArticulo + " WHERE COD1_ART = '" + datos[0, 3] + "' AND COD_ALM = '" + almacenOficial + "'";
                                    BD.GuardaCambios(guardaCatArticulo);
                                    BD.GuardaCambios(guardaExoPorAlmcen);
                                }
                            }
                            //}
                            //else
                            //{
                            //    banderaRenglones = false;
                            //}
                            //y = y + 1;
                            //}
                            //}
                        }

                        //} if (banderaRenglones == true) { folioenc = folioenc + 1; }
                        #endregion
                    }
                }
                #endregion
                //escribeArchivo(nombre);
                //--------------------------------------------------------------------------------------------------------------
                #region Version

                ////MessageBox.Show("almacenes");
                //escribe(2, "", nombre);
                //escribe(2, "", nombre);
                escribe(1, "Version", nombre);
                XmlNodeList DatosVersion = xDoc.GetElementsByTagName("DatosVersion");

                foreach (XmlElement nodo in DatosVersion)//lista)
                {

                    int i = 0;
                    XmlNodeList version = nodo.GetElementsByTagName("version");
                    XmlNodeList dependencia = nodo.GetElementsByTagName("dependencia");
                    XmlNodeList SAP = nodo.GetElementsByTagName("SAP");

                    BD.FunicionEjecucion("UPDATE tblac_dependencias SET Version_Dp=" + version[i].InnerText + "  WHERE Id_Sucursal='" + dependencia[i].InnerText + "';");
                    BD.FunicionEjecucion("UPDATE tblac_dependencias SET SAP='" + SAP[i].InnerText + "'  WHERE Id_Sucursal='" + dependencia[i].InnerText + "';");

                    try
                    {
                        XmlNodeList ip = nodo.GetElementsByTagName("ip");
                        XmlNodeList basedatosGB = nodo.GetElementsByTagName("basedatosGB");
                        XmlNodeList basedatosDP = nodo.GetElementsByTagName("basedatosDP");
                        BD.FunicionEjecucion("UPDATE tblac_dependencias SET IP_DP='" + (ip[i].InnerText.Trim()).Replace(Environment.NewLine, "") + "' WHERE Id_Sucursal='" + dependencia[i].InnerText + "';");
                        BD.FunicionEjecucion("UPDATE tblac_dependencias SET BD_DP='" + basedatosGB[i].InnerText + "'  WHERE Id_Sucursal='" + dependencia[i].InnerText + "';");
                        //BD.FunicionEjecucion("UPDATE tblac_dependencias SET SAP='" + basedatosDP[i].InnerText + "'  WHERE Id_Sucursal='" + dependencia[i].InnerText + "';");

                    }
                    catch
                    {

                    }
                }
                #endregion
                //_----------------------------------

                #region Factura Electronica

                ////MessageBox.Show("almacenes");
                //escribe(2, "", nombre);
                //escribe(2, "", nombre);
                escribe(1, "FacturacionElectronica", nombre);
                XmlNodeList FacturacionElectronica = xDoc.GetElementsByTagName("FacturacionElectronica");

                foreach (XmlElement nodo in FacturacionElectronica)//lista)
                {

                    int i = 0;

                    XmlNodeList FOLIO_INTERNO = nodo.GetElementsByTagName("FOLIO_INTERNO");
                    XmlNodeList SERIE_FISCAL = nodo.GetElementsByTagName("SERIE_FISCAL");
                    XmlNodeList FOLIO_FISCAL = nodo.GetElementsByTagName("FOLIO_FISCAL");
                    XmlNodeList NO_APROBACION = nodo.GetElementsByTagName("NO_APROBACION");
                    XmlNodeList SELLO_FE = nodo.GetElementsByTagName("SELLO_FE");
                    XmlNodeList CADENA_ORIGINAL = nodo.GetElementsByTagName("CADENA_ORIGINAL");
                    XmlNodeList ANO_APROBACION = nodo.GetElementsByTagName("ANO_APROBACION");
                    XmlNodeList TIPO_COMPROBANTE = nodo.GetElementsByTagName("TIPO_COMPROBANTE");
                    XmlNodeList ESTATUS_FE = nodo.GetElementsByTagName("ESTATUS_FE");
                    XmlNodeList MENSAJE_FE = nodo.GetElementsByTagName("MENSAJE_FE");
                    XmlNodeList NO_CERTIFICADO = nodo.GetElementsByTagName("NO_CERTIFICADO");
                    XmlNodeList TIMBRE_UUID = nodo.GetElementsByTagName("TIMBRE_UUID");
                    XmlNodeList FECHA_TIMBRADO = nodo.GetElementsByTagName("FECHA_TIMBRADO");
                    XmlNodeList SELLO_SAT = nodo.GetElementsByTagName("SELLO_SAT");
                    XmlNodeList CERTIFICADO_SAT = nodo.GetElementsByTagName("CERTIFICADO_SAT");
                    XmlNodeList REGIMENES = nodo.GetElementsByTagName("REGIMENES");
                    XmlNodeList COND_PAGO = nodo.GetElementsByTagName("COND_PAGO");
                    XmlNodeList METODO_PAGO = nodo.GetElementsByTagName("METODO_PAGO");
                    XmlNodeList NUMCTA_PAGO = nodo.GetElementsByTagName("NUMCTA_PAGO");
                    XmlNodeList LUGAR_EXPED = nodo.GetElementsByTagName("LUGAR_EXPED");
                    XmlNodeList TIPO_MONEDA = nodo.GetElementsByTagName("TIPO_MONEDA");
                    XmlNodeList CONCILIADA = nodo.GetElementsByTagName("CONCILIADA");
                    XmlNodeList CVEPAGO_SAT = nodo.GetElementsByTagName("CVEPAGO_SAT");
                    XmlNodeList FECHA_CANCELA = nodo.GetElementsByTagName("FECHA_CANCELA");

                    string sentencia = "";
                    if (BD.consulta("SELECT COUNT(*) FROM tblFacturaElectronica WHERE FOLIO_INTERNO ='" + FOLIO_INTERNO[i].InnerText + "'") == "0")
                    {
                        sentencia = "INSERT INTO tblFacturaElectronica (FOLIO_INTERNO ,SERIE_FISCAL ,FOLIO_FISCAL ,NO_APROBACION ,SELLO_FE ,CADENA_ORIGINAL ,ANO_APROBACION ,TIPO_COMPROBANTE ,ESTATUS_FE ,MENSAJE_FE ,NO_CERTIFICADO ,TIMBRE_UUID ,FECHA_TIMBRADO ,SELLO_SAT ,CERTIFICADO_SAT ,REGIMENES ,COND_PAGO ,METODO_PAGO ,NUMCTA_PAGO ,LUGAR_EXPED ,TIPO_MONEDA ,CONCILIADA ,CVEPAGO_SAT, FECHA_CANCELA) values " +
                            "('" + FOLIO_INTERNO[i].InnerText + "', '" + SERIE_FISCAL[i].InnerText + "', '" + FOLIO_FISCAL[i].InnerText + "', '" + NO_APROBACION[i].InnerText + "', '" + SELLO_FE[i].InnerText + "', '" + CADENA_ORIGINAL[i].InnerText + "', " + ANO_APROBACION[i].InnerText + ", " + TIPO_COMPROBANTE[i].InnerText + ", " + ESTATUS_FE[i].InnerText + ", '" + MENSAJE_FE[i].InnerText + "', '" + NO_CERTIFICADO[i].InnerText + "', '" + TIMBRE_UUID[i].InnerText + "', '" + FECHA_TIMBRADO[i].InnerText + "', '" + SELLO_SAT[i].InnerText + "', '" + CERTIFICADO_SAT[i].InnerText + "', '" + REGIMENES[i].InnerText + "', '" + COND_PAGO[i].InnerText + "', '" + METODO_PAGO[i].InnerText + "', '" + NUMCTA_PAGO[i].InnerText + "', " + LUGAR_EXPED[i].InnerText + ", '" + TIPO_MONEDA[i].InnerText + "', " + CONCILIADA[i].InnerText + ", '" + CVEPAGO_SAT[i].InnerText + "', '" + FECHA_CANCELA[i].InnerText + "')";
                    }
                    else
                    {
                        sentencia = "UPDATE tblFacturaElectronica SET " +
                            " SERIE_FISCAL='" + SERIE_FISCAL[i].InnerText + "', FOLIO_FISCAL='" + FOLIO_FISCAL[i].InnerText + "', NO_APROBACION='" + NO_APROBACION[i].InnerText + "', SELLO_FE='" + SELLO_FE[i].InnerText + "', CADENA_ORIGINAL='" + CADENA_ORIGINAL[i].InnerText + "', ANO_APROBACION=" + ANO_APROBACION[i].InnerText + ", TIPO_COMPROBANTE=" + TIPO_COMPROBANTE[i].InnerText + ", ESTATUS_FE=" + ESTATUS_FE[i].InnerText + ", MENSAJE_FE='" + MENSAJE_FE[i].InnerText + "', NO_CERTIFICADO='" + NO_CERTIFICADO[i].InnerText + "', TIMBRE_UUID='" + TIMBRE_UUID[i].InnerText + "', FECHA_TIMBRADO='" + FECHA_TIMBRADO[i].InnerText + "', SELLO_SAT='" + SELLO_SAT[i].InnerText + "', CERTIFICADO_SAT='" + CERTIFICADO_SAT[i].InnerText + "', REGIMENES='" + REGIMENES[i].InnerText + "', COND_PAGO='" + COND_PAGO[i].InnerText + "', METODO_PAGO='" + METODO_PAGO[i].InnerText + "', NUMCTA_PAGO='" + NUMCTA_PAGO[i].InnerText + "', LUGAR_EXPED=" + LUGAR_EXPED[i].InnerText + ", TIPO_MONEDA='" + TIPO_MONEDA[i].InnerText + "', CONCILIADA=" + CONCILIADA[i].InnerText + ", CVEPAGO_SAT='" + CVEPAGO_SAT[i].InnerText + "' , FECHA_CANCELA='" + FECHA_CANCELA[i].InnerText + "' " +
                            " WHERE FOLIO_INTERNO='" + FOLIO_INTERNO[i].InnerText + "';";

                    }
                    BD.GuardaCambios(sentencia);

                }
                #endregion
                //----------------------------
                #region Bitacora Antibiotico

                ////MessageBox.Show("almacenes");
                //escribe(2, "", nombre);
                //escribe(2, "", nombre);
                escribe(1, "Bitacora Antibiotico", nombre);
                XmlNodeList BitacoraAntibioticos = xDoc.GetElementsByTagName("BitacoraAntibioticos");

                foreach (XmlElement nodo in BitacoraAntibioticos)//lista)
                {

                    int i = 0;

                    XmlNodeList ACCION = nodo.GetElementsByTagName("ACCION");
                    XmlNodeList FOLIO = nodo.GetElementsByTagName("FOLIO");
                    XmlNodeList FECHA = nodo.GetElementsByTagName("FECHA");
                    XmlNodeList HORA = nodo.GetElementsByTagName("HORA");
                    XmlNodeList COD_USU = nodo.GetElementsByTagName("COD_USU");
                    XmlNodeList CAJA = nodo.GetElementsByTagName("CAJA");
                    XmlNodeList COD1_ART = nodo.GetElementsByTagName("COD1_ART");
                    XmlNodeList CAN_ART = nodo.GetElementsByTagName("CAN_ART");
                    XmlNodeList PARCIAL = nodo.GetElementsByTagName("PARCIAL");
                    XmlNodeList NUM_RECETA = nodo.GetElementsByTagName("NUM_RECETA");
                    XmlNodeList CED_RECETA = nodo.GetElementsByTagName("CED_RECETA");
                    XmlNodeList MED_RECETA = nodo.GetElementsByTagName("MED_RECETA");
                    XmlNodeList DIR_RECETA = nodo.GetElementsByTagName("DIR_RECETA");
                    XmlNodeList NOTAS = nodo.GetElementsByTagName("NOTAS");
                    XmlNodeList COD_SUCU = nodo.GetElementsByTagName("COD_SUCU");


                    //se agregaron los '' en el campo accion 20231005
                    string sentencia = "INSERT INTO tblBitacoraAntibioticos  " +
                        "(ACCION, FOLIO, FECHA, HORA, COD_USU, CAJA, COD1_ART, CAN_ART, PARCIAL, NUM_RECETA, CED_RECETA, MED_RECETA, DIR_RECETA, NOTAS,COD_SUCU, ENVIADO) values ('" +
                        ACCION[i].InnerText + "', '" + FOLIO[i].InnerText + "', '" + CambioDeFecha(FECHA[i].InnerText) + "', '" + HORA[i].InnerText + "', '" + COD_USU[i].InnerText + "', " +
                        CAJA[i].InnerText + ", '" + COD1_ART[i].InnerText + "', "+ CAN_ART[i].InnerText +", "+ PARCIAL[i].InnerText +", '" + NUM_RECETA[i].InnerText + "', '" +
                        CED_RECETA[i].InnerText + "', '" + MED_RECETA[i].InnerText + "', '" + DIR_RECETA[i].InnerText +"', '" + NOTAS[i].InnerText + "' , '"+ COD_SUCU[i].InnerText + "', 0)";
                    BD.GuardaCambios(sentencia);

                }
                #endregion
                //-----------------------------

                #region Bitacora CDSO

                ////MessageBox.Show("almacenes");
                //escribe(2, "", nombre);
                //escribe(2, "", nombre);
                escribe(1, "Bitacora CDSO", nombre);
                XmlNodeList BitacoraCDSO = xDoc.GetElementsByTagName("BitacoraCDSO");

                foreach (XmlElement nodo in BitacoraCDSO)//lista)
                {

                    int i = 0;

                    XmlNodeList SESION = nodo.GetElementsByTagName("SESION");
                    XmlNodeList ACCION = nodo.GetElementsByTagName("ACCION");
                    XmlNodeList NUM_TARJETA = nodo.GetElementsByTagName("NUM_TARJETA");
                    XmlNodeList FECHA = nodo.GetElementsByTagName("FECHA");
                    XmlNodeList HORA = nodo.GetElementsByTagName("HORA");
                    XmlNodeList FOLIO_NV = nodo.GetElementsByTagName("FOLIO_NV");
                    XmlNodeList COD_USU = nodo.GetElementsByTagName("COD_USU");
                    XmlNodeList CAJA = nodo.GetElementsByTagName("CAJA");
                    XmlNodeList RESP_ERR = nodo.GetElementsByTagName("RESP_ERR");
                    XmlNodeList RESP_MSG = nodo.GetElementsByTagName("RESP_MSG");
                    XmlNodeList COD1_ART = nodo.GetElementsByTagName("COD1_ART");
                    XmlNodeList PMP = nodo.GetElementsByTagName("PMP");
                    XmlNodeList PCIO_ART = nodo.GetElementsByTagName("PCIO_ART");
                    XmlNodeList PZAS_PAG = nodo.GetElementsByTagName("PZAS_PAG");
                    XmlNodeList PZAS_GRATIS = nodo.GetElementsByTagName("PZAS_GRATIS");
                    XmlNodeList PCIO_FIJO = nodo.GetElementsByTagName("PCIO_FIJO");
                    XmlNodeList POR_DESC = nodo.GetElementsByTagName("POR_DESC");
                    XmlNodeList IMP_DESC = nodo.GetElementsByTagName("IMP_DESC");
                    XmlNodeList TIPO_DESC = nodo.GetElementsByTagName("TIPO_DESC");
                    XmlNodeList FOL_AUTORIZA = nodo.GetElementsByTagName("FOL_AUTORIZA");
                    XmlNodeList COD_SUCU = nodo.GetElementsByTagName("COD_SUCU");

                    string sentencia = "INSERT INTO tblBitacoraCDSO (sesion, accion, num_tarjeta, fecha, hora, folio_nv, cod_usu, caja, resp_err, resp_msg, " +
                 " cod1_art, pmp, pcio_art, pzas_pag, pzas_gratis, pcio_fijo, por_desc, imp_desc, tipo_desc, FOL_AUTORIZA, COD_SUCU, ENVIADO) values " +
                 "('" + SESION[i].InnerText + "', " + ACCION[i].InnerText + ", '" + NUM_TARJETA[i].InnerText + "', '" + CambioDeFecha(FECHA[i].InnerText) + "', '" + HORA[i].InnerText + "', " +
                 "'" + FOLIO_NV[i].InnerText + "','" + COD_USU[i].InnerText + "', " + CAJA[i].InnerText + ", " + RESP_ERR[i].InnerText + ", '" + RESP_MSG[i].InnerText + "', " +
                 "'" + COD1_ART[i].InnerText + "', " + PMP[i].InnerText + ", " + PCIO_ART[i].InnerText + ", " + PZAS_PAG[i].InnerText + ", " +
                 " " + PZAS_GRATIS[i].InnerText + ", " + PCIO_FIJO[i].InnerText + ", " + POR_DESC[i].InnerText + ", " + IMP_DESC[i].InnerText + ", 0" + TIPO_DESC[i].InnerText + ", " +
                 "'" + FOL_AUTORIZA[i].InnerText + "', '" + COD_SUCU[i].InnerText + "', 0)";
                 BD.GuardaCambios(sentencia);

                }
                #endregion

                //-----------------------------

                #region Existencia
                escribe(1, "Bitacora CDSO", nombre);
                XmlNodeList Existencia = xDoc.GetElementsByTagName("Existencia");

                if (Existencia.Count > 0)
                {
                    lista = ((XmlElement)Existencia[0]).GetElementsByTagName("ExistencianArt");
                    foreach (XmlElement nodo in lista)//lista)
                    {

                        int i = 0;


                        XmlNodeList COD1_ART = nodo.GetElementsByTagName("COD1_ART");
                        XmlNodeList EXI_ALM = nodo.GetElementsByTagName("EXI_ALM");
                        XmlNodeList COD_ALM = nodo.GetElementsByTagName("COD_ALM");
                        XmlNodeList COD_DP = nodo.GetElementsByTagName("COD_DP");


                        string sentencia = " UPDAtE tblAC_Existencias SET CANDTIDAD = " + EXI_ALM[i].InnerText + " " +
                        "WHERE COD_ART='" + COD1_ART[i].InnerText + "' AND COD_ALM ='" + COD_ALM[i].InnerText + "' AND COD_DP='" + COD_DP[i].InnerText + "';";
                        BD.GuardaCambios(sentencia);

                    }
                }
                #endregion


                status = "3"; sbandera ="";
                //}
            }
            catch (System.OutOfMemoryException ex)
            {
                error = true; status = "2";
                mensajeHilo = "hilo: " + hilo + " " + ex.Message + " " + ex.InnerException + " " + ex.StackTrace;
            }
            catch (Exception err)
            {
                //var st = new StackTrace(err, true);
                //var frame = st.GetFrame(0);

                //LineaError = err.StackTrace;

                #region Opciones
                //string MachineName = System.Environment.MachineName;
                //string UserName = System.Environment.UserName.ToUpper();
                //string Mensaje = ex.Message;
                //int LineaError = frame.GetFileLineNumber();
                //string Proyecto = frame.GetMethod().Module.Assembly.GetName().Name;
                //string Clase = frame.GetMethod().DeclaringType.Name;
                //string metodo = frame.GetMethod().Name;
                //string codigoError = frame.GetHashCode();    
                #endregion


                error = true; status = "99";
                mensajeHilo = "hilo: " + hilo + " " + err.Message + " " + err.InnerException + " " + err.StackTrace;
            }
            finally
            {
                escribe(1, " ", nombre);
                if (sbandera.Length > 0)
                    escribe(1, "Bandera en - " + sbandera + " - BD error: " + funcionBD.mensaje + "  -  Error: " + mensajeHilo + Environment.NewLine, nombre);
                
                escribe(1, "Fin de lectura - " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"), nombre);
                escribeArchivo(nombre);
                mensajeSentencia = "";
                mensajeSentencia = "---" + BD.mensajeSententencia;
                BD.conexionMySQL.Dispose();
                BD.conexionMSSQL.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void ResizeArray(ref string[,] Arr, int x)
        {
            string[,] _arr = new string[x, 5];
            int minRows = Math.Min(x, Arr.GetLength(0));
            int minCols = Math.Min(5, Arr.GetLength(1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    _arr[i, j] = Arr[i, j];
            Arr = _arr;
        }
        //quitar los convert 1 y 2 verificar
        private decimal verificaLongitud(string numero)
        {
            decimal respuesta = 0;
            if (Convert.ToDecimal(numero) > Convert.ToDecimal(999999999.9999))
            {
                respuesta = Convert.ToDecimal(999999999.9999);
            }
            else
            {
                respuesta = Convert.ToDecimal(numero);
            }

            return respuesta;
        }

        public void Dispose()
        {

        }

        private void GrabarCapaDeCostos(string sReferenciaMov, string sFechaMov, string sTipoDeMovimiento, string sCod1Articulo,
                                      decimal cCantidadArt, string sCodigoUnidad, decimal cEquivalenciaArt, decimal cCostoUnitario, int iCodMonedaMov, decimal cTipoDeCambioArticulo,
                                      string sCodAlmPrincipal, decimal cSalidaArticulo, decimal cExistenciaCapa, string sNumeroDeLote, string dFechaCaducidad, string numPedimento,  string dFechaPedimento)
        {
            string sentencia = "";
            funcionBD BD = new funcionBD();
            //Se agrego el campo FEC_PEDIM con su default 20231005
            if (sNumeroDeLote.Length > 0)
            {
                sentencia = "INSERT INTO tblCostos (REF_MOV,FEC_MOV,COD1_ART,CAN_ART,COD_UND,EQV_UND,COS_UNI,COD_MON,TIP_CAM,COD_ALM,SAL_ART,EXI_ART,TIP_MOV,NUM_LOT,FEC_CAD, NUM_PEDIM, FEC_PEDIM) VALUES ('" +
                             sReferenciaMov + "'," + sFechaMov + ",'" + sCod1Articulo + "'," + cCantidadArt + ",'" + sCodigoUnidad + "'," + cEquivalenciaArt + "," + cCostoUnitario + "," + iCodMonedaMov + "," + cTipoDeCambioArticulo + ",'" + sCodAlmPrincipal + "'," + cSalidaArticulo + "," + cExistenciaCapa + ",'" + sTipoDeMovimiento + "','" + sNumeroDeLote + "'," + dFechaCaducidad + ",'" + numPedimento + "', '" + dFechaPedimento + "');";
            }
            else
            {
                sentencia = "INSERT INTO tblCostos (REF_MOV,FEC_MOV,COD1_ART,CAN_ART,COD_UND,EQV_UND,COS_UNI,COD_MON,TIP_CAM,COD_ALM,SAL_ART,EXI_ART,TIP_MOV,NUM_LOT,FEC_CAD, NUM_PEDIM, FEC_PEDIM) VALUES ('" +
                            sReferenciaMov + "'," + sFechaMov + ",'" + sCod1Articulo + "'," + cCantidadArt + ",'" + sCodigoUnidad + "'," + cEquivalenciaArt + "," + cCostoUnitario + "," + iCodMonedaMov + "," + cTipoDeCambioArticulo + ",'" + sCodAlmPrincipal + "'," + cSalidaArticulo + "," + cExistenciaCapa + ",'" + sTipoDeMovimiento + "',''," + dFechaCaducidad + ", '" + numPedimento + "', '" + dFechaPedimento + "');";
            }
            BD.GuardaCambios(sentencia);
        }

        private void SalidaPromedioNET(string CodigoAlmacen, string FolioDeSalida, decimal cCosPEPS, string sCod1Art)
        {
            funcionBD BD = new funcionBD();
            Int64 lNumMovim = 0;
            decimal cExiArt = 0; decimal cEqvArt = 0; decimal cCantSali = 0;
            string sRefCapa = ""; string sentencia = ""; string sentencia2 = "";

            decimal cSalArt = 0; decimal cCosEq = 0;
            decimal cCosRef = 0; string dFecSal;

            string NUM_LOT = "";
            string FEC_CAD =  "18000101";
            string NUM_PEDIM = "";
            string FEC_PEDIM = "18000101";
                    

            DataTable tabla1 = new DataTable(); DataTable tabla2 = new DataTable(); DataTable tabla3 = new DataTable(); DataTable rstMovEntSal = new DataTable();
            rstMovEntSal.Columns.Add("NUM_ES", typeof(String));

            sentencia = "SELECT NUM_MOV,EXI_ART,COS_UNI,REF_MOV,EQV_UND FROM tblCostos WHERE (COD1_ART='" + sCod1Art + "' AND TIP_MOV='EALM' AND COD_ALM='" + CodigoAlmacen + "' AND EXI_ART>0) ORDER BY FEC_MOV,NUM_MOV;";
            tabla1 = BD.datatableBD(sentencia);
            foreach (DataRow row in tabla1.Rows)
            {
                lNumMovim = Convert.ToInt64(row["NUM_MOV"].ToString());
                cExiArt = Convert.ToDecimal(row["EXI_ART"].ToString());
                sRefCapa = row["REF_MOV"].ToString();
                cEqvArt = Convert.ToDecimal(row["EQV_UND"].ToString());
                cCosEq = Convert.ToDecimal(row["COS_UNI"].ToString());
                cCosRef = 0;
                cCosRef = Convert.ToDecimal(row["COS_UNI"].ToString());
                //20231006 Se agregaron al querry NUM_LOT, FEC_CAD, NUM_PEDIM, FEC_PEDIM      
                sentencia2 = "SELECT NUM_MOV,EXI_ART,COS_UNI,EQV_UND, NUM_LOT, FEC_CAD, NUM_PEDIM, FEC_PEDIM FROM tblCostos WHERE (REF_MOV='" + FolioDeSalida + "' AND COD1_ART='" + sCod1Art + "');";
                tabla2 = BD.datatableBD(sentencia2);
                foreach (DataRow rows in tabla2.Rows)
                {
                    if (Convert.ToDecimal(row["EXI_ART"].ToString()) > 0)
                    {
                        cCantSali = Convert.ToDecimal(row["EXI_ART"].ToString());
                        if (cCantSali >= cExiArt)
                        {
                            cSalArt = cExiArt; cExiArt = 0;
                        }
                        else
                        {
                            cSalArt = cCantSali; cExiArt = 0;
                        }
                    }
                    cCosPEPS = cCosPEPS + (cSalArt * (cCosEq / cEqvArt));
                    dFecSal = BD.BuscaRegistroConVariasCondiciones("SELECT CONVERT(VARCHAR(10), FEC_MOV, 112) FROM tblGralAlmacen WHERE (REF_MOV='" + FolioDeSalida + "');");
                    if (dFecSal.Length == 0)
                    {
                        dFecSal = DateTime.Now.ToString("yyyy/MM/dd");
                    }

                    BD.FunicionEjecucion("UPDATE tblCostos SET SAL_ART=SAL_ART + " + cSalArt + ",EXI_ART=EXI_ART - " + cSalArt + " WHERE NUM_MOV=" + lNumMovim + ";");

                    #region 20231006
                    {
                        NUM_LOT = !String.IsNullOrEmpty(rows["NUM_LOT"].ToString()) ? rows["NUM_LOT"].ToString() : "";
                        FEC_CAD = !String.IsNullOrEmpty(rows["FEC_CAD"].ToString()) ? Convert.ToDateTime(rows["FEC_CAD"]).ToString("yyyyMMdd") : "18000101";
                        NUM_PEDIM = !String.IsNullOrEmpty(rows["NUM_PEDIM"].ToString()) ? rows["NUM_PEDIM"].ToString() : "";
                        FEC_PEDIM = !String.IsNullOrEmpty(rows["FEC_PEDIM"].ToString()) ? Convert.ToDateTime(rows["FEC_PEDIM"]).ToString("yyyyMMdd") : "18000101";
                    }
                    #endregion


                    BD.FunicionEjecucion("INSERT INTO tblAdicionalCapas (NUM_ENT,REF_ENT,CAN_SAL,REF_SAL,COD1_ART,FEC_SAL,COS_REF, NUM_LOT, FEC_CAD, NUM_PEDIM, FEC_PEDIM) VALUES (" + lNumMovim + ",'" + sRefCapa + "'," + cSalArt + ",'" + FolioDeSalida + "','" + sCod1Art + "','" + CambioDeFecha(dFecSal) + "'," + cCosRef + ", '" + NUM_LOT + "', '" + FEC_CAD + "', '" + NUM_PEDIM + "', '" + FEC_PEDIM + "');");

                    sentencia = "SELECT EXI_ART FROM tblCostos WHERE (NUM_MOV=" + lNumMovim + ");";
                    tabla3 = BD.datatableBD(sentencia);
                    foreach (DataRow rowz in tabla3.Rows)
                    {
                        if (Convert.ToDecimal(rowz["EXI_ART"].ToString()) <= 0)
                        {
                            rstMovEntSal.Rows.Add(lNumMovim.ToString());
                        }
                    }
                    lNumMovim = Convert.ToInt64(rows["NUM_MOV"].ToString());
                    BD.FunicionEjecucion("UPDATE tblCostos SET SAL_ART=SAL_ART + " + cSalArt + ",EXI_ART=EXI_ART - " + cSalArt + " WHERE NUM_MOV=" + lNumMovim + ";");

                    sentencia = "SELECT EXI_ART FROM tblCostos WHERE (NUM_MOV=" + lNumMovim + ");";
                    tabla3 = BD.datatableBD(sentencia);
                    foreach (DataRow rowz in tabla3.Rows)
                    {
                        if (Convert.ToDecimal(rowz["EXI_ART"].ToString()) <= 0)
                        {
                            rstMovEntSal.Rows.Add(lNumMovim.ToString());
                        }
                    }
                }
            }

            foreach (DataRow row in rstMovEntSal.Rows)
            {
                BD.FunicionEjecucion("DELETE FROM tblCostos WHERE (NUM_MOV=" + row["NUM_ES"].ToString() + ");");

            }

        }


        private void CalCostoPromedioNET(string sCod1Art, string sCadenaDeConexionPS, string sFecha)
        {
            funcionBD BD = new funcionBD();
            decimal cCantidad = 0, cCosTot = 0;
            DataTable rstAlmacen = new DataTable();

            rstAlmacen = BD.datatableBD("SELECT EXI_ART,EQV_UND,COS_UNI FROM tblCostos WHERE ((COD1_ART='" + sCod1Art + "' AND TIP_MOV='EALM') AND EXI_ART>0);");
            foreach (DataRow row in rstAlmacen.Rows)
            {
                cCantidad = cCantidad + Convert.ToDecimal(row["EXI_ART"].ToString());
                if (Convert.ToDecimal(row["EQV_UND"].ToString()) == 0)
                {
                    cCosTot = cCosTot + (Convert.ToDecimal(row["EXI_ART"].ToString()) * Convert.ToDecimal(row["COS_UNI"].ToString()));
                }
                else
                {
                    cCosTot = cCosTot + ((Convert.ToDecimal(row["EXI_ART"].ToString()) / Convert.ToDecimal(row["EQV_UND"].ToString())) * Convert.ToDecimal(row["COS_UNI"].ToString()));
                }
            }

            if (cCantidad != 0 || cCosTot != 0)
            {
                rstAlmacen = BD.datatableBD("SELECT COS_PRO,COD_UND,EQV_UND FROM tblUndCosPreArt WHERE (COD1_ART='" + sCod1Art + "');");
                foreach (DataRow row in rstAlmacen.Rows)
                {
                    BD.FunicionEjecucion("UPDATE tblUndCosPreArt SET COS_PRO=" + ((cCosTot / cCantidad) * Convert.ToDecimal(row["EQV_UND"].ToString())) + " WHERE (COD1_ART='" + sCod1Art + "' AND COD_UND='" + row["COD_UND"].ToString() + "');");
                }
                if (sFecha.Length > 0)
                {
                    rstAlmacen = BD.datatableBD("SELECT * FROM tblPromedios WHERE COD1_ART='" + sCod1Art + "' AND FEC_PROM=" + sFecha + ";");
                    if (rstAlmacen.Rows.Count > 0)
                    {
                        foreach (DataRow row in rstAlmacen.Rows)
                        {
                            BD.FunicionEjecucion("UPDATE tblPromedios SET COS_PROM=" + (cCosTot / cCantidad) + " WHERE COD1_ART='" + sCod1Art + "' AND FEC_PROM=" + sFecha + ";");
                        }
                    }
                    else
                    {
                        BD.FunicionEjecucion("INSERT INTO tblPromedios (COD1_ART,FEC_PROM,COS_PROM) VALUES ('" + sCod1Art + "'," + sFecha + "," + (cCosTot / cCantidad) + ");");
                    }
                }
            }

        }




    }
}
