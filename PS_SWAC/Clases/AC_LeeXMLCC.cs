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
using PS_SWAC.Clases;

namespace PS_PACIFIC.Clases
{
    class AC_LeeXMLCC : IDisposable
    {

        public string cnnxion, mensajeSentencia, archivo;

        public int nv = 0, renglon = 0, auxcaja = 0; public string mensajeHilo = "", status = "";
        public string nombre = ""; public bool error = false;
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
            
        }

          private  string CambioDeFecha(string fechax)
        {
            string date = "";
            if (fechax.Length > 0)
            {
                try
                {

                    if (fechax.Length > 8)
                    {
                        string[] fech = fechax.Split(' ');
                        var dates = DateTime.ParseExact(fech[0],
                            new[] { "M/d/yyyy", "M-d-yyyy", "yyyy-M-d", "yyyy/M/d", "d/M/yyyy", "d-M-yyyy" },
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None);
                        date = dates.ToString("yyyyMMdd");
                    }
                    else
                    {
                        date = fechax;
                    }
                }
                catch (Exception err)
                {
                    throw new System.ArgumentException("Fecha " + err.Message, err.InnerException + "CambioDeFecha(string fechax) " + fechax + "<-->" + date);
                }

            }
            else { date = "18000101"; }

            return date;
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

            string NombrePaq = "";
            //PS_FuncionesVB.clsCostos objCostos = new PS_FuncionesVB.clsCostos();
            Decimal cCosPEPS;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(archivoXML);
            string nombre = archivoXML;
            string[] nombretxt;
            nombretxt = nombre.Split(Convert.ToChar(@"\"));
            nombretxt[0] = nombretxt[nombretxt.Length - 1].Substring(0, nombretxt[nombretxt.Length - 1].Length - 4);
            string ruta;
            ruta = rutaPath;// BD.BuscaRegistroConVariasCondiciones("SELECT Ruta_Paquetes FROM tblac_config", BD.conexionAC);
            nombre = funciones.ReemplazarCadena(ruta, "/", @"\");
            nombre = nombre + @"\" + nombretxt[0] + ".txt";
            archivo = nombre;
            //20151104 
            string fechaTimestre = "2000-01-01"; //BD.consulta("SELECT FECFIN_TRIM FROM BitacoraHistoricos")
            DateTime fechaTrimestre = Convert.ToDateTime(fechaTimestre);
            StreamWriter writer = File.CreateText(nombre);
            writer.Close();

            DataTable dtUsuarios = BD.datatableBD("SELECT COD_USU FROM tblUsuarios;");
            DataTable dtCajas = BD.datatableBD("SELECT COD_CAJ FROM tblCajas;");
            DataTable dtImpuestos = BD.datatableBD("SELECT COD_IMP FROM tblImpuestos");
            DataTable dtMoneda = BD.datatableBD("SELECT COD_MON, TIP_CAM FROM tblMonedas");
            DataTable dtCatAlmacenes = BD.datatableBD("SELECT COD_ALM FROM tblCatAlmacenes");

            string[,] DatosFactura = new string[1, 27];
            string[,] DatosNotaDeVenta1 = new string[1, 31];
            string[,] DatosNotaDeVenta2 = new string[1, 42];
            string[,] DatosNotaDeVenta3 = new string[1, 31];
            string[,] DatosNotaDeDevolucion = new string[1, 31];
            string[,] RengloNotaDeDevolucion = new string[1, 18];

            string[,] Generales_Cliente = new string[1, 39];

            string[,] DatosFactura1 = new string[1, 28];

            XmlNodeList lista;

            try
            {
                error = false; mensajeHilo = "";
                escribe(1, "hilo: " + hilo + " Inicio de lectura - " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"), nombre);
                escribeArchivo(nombre);


                //------Tipo 1
                escribe(1, "Tipo 1", nombre);

                #region Tipo 1
                XmlNodeList Tipo1 = xDoc.GetElementsByTagName("Tipo1");

                XmlNodeList listaT1;
                foreach (XmlElement nodo in Tipo1)
                {
                    #region Ciudad
                    escribe(1, "Ciudad", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblCiudad");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Ciudad");
                        foreach (XmlElement nodo2 in listaN1)
                        {
                            XmlNodeList COD_CID = nodo2.GetElementsByTagName("codigo_ciudad_cte");
                            XmlNodeList NOM_CID = nodo2.GetElementsByTagName("desc_ciudad_cte");
                            XmlNodeList DES_CID = nodo2.GetElementsByTagName("abr_ciudad_cte");
                            XmlNodeList COD_AREA = nodo2.GetElementsByTagName("area_ciudad_cte");
                            XmlNodeList MUN_CID = nodo2.GetElementsByTagName("municipio");

                            if (BD.consulta("SELECT COUNT(*) FROM tblCiudad WHERE COD_CID ='" + COD_CID[0].InnerText + "'") == "0")
                            {
                                BD.FunicionEjecucion("INSERT INTO tblCiudad(COD_CID, DES_CID, NOM_CID, COD_AREA, MUN_CID) VALUES ('" +
                                     COD_CID[0].InnerText + "', '" + NOM_CID[0].InnerText + "', '" + DES_CID[0].InnerText + "', '" + COD_AREA[0].InnerText + "', '" + MUN_CID[0].InnerText + "')");
                            }
                            else
                            {
                                BD.FunicionEjecucion("UPDATE tblCiudad SET DES_CID = '" + NOM_CID[0].InnerText + "', NOM_CID = '" + DES_CID[0].InnerText + "', COD_AREA = '" + COD_AREA[0].InnerText + "', MUN_CID = '" + MUN_CID[0].InnerText + "' WHERE COD_CID ='" + COD_CID[0].InnerText + "'");
                            }
                        }
                    }
                    #endregion

                    #region CatClientes
                    escribe(1, "CatClientes", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblCatClientes");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Generales_Cliente");
                        foreach (XmlElement nodo2 in listaN1)
                        {
                            int i = 0, z = 0;

                            XmlNodeList Generales_Cliente1 = nodo1.GetElementsByTagName("codigo_cte");
                            XmlNodeList Generales_Cliente2 = nodo1.GetElementsByTagName("nombre_cte");
                            XmlNodeList Generales_Cliente3 = nodo1.GetElementsByTagName("direccion_cte");
                            XmlNodeList Generales_Cliente4 = nodo1.GetElementsByTagName("colonia_cte");
                            XmlNodeList Generales_Cliente5 = nodo1.GetElementsByTagName("cp_cte");
                            XmlNodeList Generales_Cliente6 = nodo1.GetElementsByTagName("ciudad_cte");
                            XmlNodeList Generales_Cliente7 = nodo1.GetElementsByTagName("estado_cte");
                            XmlNodeList Generales_Cliente8 = nodo1.GetElementsByTagName("pais_cte");
                            XmlNodeList Generales_Cliente9 = nodo1.GetElementsByTagName("telefono1_cte");
                            XmlNodeList Generales_Cliente10 = nodo1.GetElementsByTagName("telefono2_cte");
                            XmlNodeList Generales_Cliente11 = nodo1.GetElementsByTagName("fax_cte");
                            XmlNodeList Generales_Cliente12 = nodo1.GetElementsByTagName("email_cte");
                            XmlNodeList Generales_Cliente13 = nodo1.GetElementsByTagName("rfc_cte");
                            XmlNodeList Generales_Cliente14 = nodo1.GetElementsByTagName("curp_cte");
                            XmlNodeList Generales_Cliente15 = nodo1.GetElementsByTagName("grupo_cte");
                            XmlNodeList Generales_Cliente16 = nodo1.GetElementsByTagName("credito_cte");
                            XmlNodeList Generales_Cliente17 = nodo1.GetElementsByTagName("plazo_cte");
                            XmlNodeList Generales_Cliente18 = nodo1.GetElementsByTagName("descuento_cte");
                            XmlNodeList Generales_Cliente19 = nodo1.GetElementsByTagName("estatus_cte");
                            XmlNodeList Generales_Cliente20 = nodo1.GetElementsByTagName("imagen_cte");
                            XmlNodeList Generales_Cliente21 = nodo1.GetElementsByTagName("ruta_cte");
                            XmlNodeList Generales_Cliente22 = nodo1.GetElementsByTagName("lista_cte");
                            XmlNodeList Generales_Cliente23 = nodo1.GetElementsByTagName("contacto1_cte");
                            XmlNodeList Generales_Cliente24 = nodo1.GetElementsByTagName("puesto1_cte");
                            XmlNodeList Generales_Cliente25 = nodo1.GetElementsByTagName("contacto2_cte");
                            XmlNodeList Generales_Cliente26 = nodo1.GetElementsByTagName("puesto2_cte");
                            XmlNodeList Generales_Cliente27 = nodo1.GetElementsByTagName("contacto3_cte");
                            XmlNodeList Generales_Cliente28 = nodo1.GetElementsByTagName("puesto3_cte");
                            XmlNodeList Generales_Cliente29 = nodo1.GetElementsByTagName("contacto4_cte");
                            XmlNodeList Generales_Cliente30 = nodo1.GetElementsByTagName("puesto4_cte");
                            XmlNodeList Generales_Cliente31 = nodo1.GetElementsByTagName("metodo_pago");
                            XmlNodeList Generales_Cliente32 = nodo1.GetElementsByTagName("cuenta_pago");
                            XmlNodeList Generales_Cliente33 = nodo1.GetElementsByTagName("numero_ext");
                            XmlNodeList Generales_Cliente34 = nodo1.GetElementsByTagName("numero_int");
                            XmlNodeList Generales_Cliente35 = nodo1.GetElementsByTagName("mail_cfd");
                            XmlNodeList Generales_Cliente36 = nodo1.GetElementsByTagName("nom_com");
                            XmlNodeList Generales_Cliente37 = nodo1.GetElementsByTagName("dp_cli");
                            XmlNodeList Generales_Cliente38 = nodo1.GetElementsByTagName("rif_cfdi");
                            XmlNodeList Generales_Cliente39 = nodo1.GetElementsByTagName("ID_CTAPAGO");

                            Generales_Cliente[z, 0] = Generales_Cliente1[i].InnerText;
                            Generales_Cliente[z, 1] = Generales_Cliente2[i].InnerText;
                            Generales_Cliente[z, 2] = Generales_Cliente3[i].InnerText;
                            Generales_Cliente[z, 3] = Generales_Cliente4[i].InnerText;
                            Generales_Cliente[z, 4] = Generales_Cliente5[i].InnerText;
                            Generales_Cliente[z, 5] = Generales_Cliente6[i].InnerText;
                            Generales_Cliente[z, 6] = Generales_Cliente7[i].InnerText;
                            Generales_Cliente[z, 7] = Generales_Cliente8[i].InnerText;
                            Generales_Cliente[z, 8] = Generales_Cliente9[i].InnerText;
                            Generales_Cliente[z, 9] = Generales_Cliente10[i].InnerText;
                            Generales_Cliente[z, 10] = Generales_Cliente11[i].InnerText;
                            Generales_Cliente[z, 11] = Generales_Cliente12[i].InnerText;
                            Generales_Cliente[z, 12] = Generales_Cliente13[i].InnerText;
                            Generales_Cliente[z, 13] = Generales_Cliente14[i].InnerText;
                            Generales_Cliente[z, 14] = Generales_Cliente15[i].InnerText;
                            Generales_Cliente[z, 15] = Generales_Cliente16[i].InnerText;
                            Generales_Cliente[z, 16] = Generales_Cliente17[i].InnerText;
                            Generales_Cliente[z, 17] = Generales_Cliente18[i].InnerText;
                            Generales_Cliente[z, 18] = Generales_Cliente19[i].InnerText;
                            Generales_Cliente[z, 19] = Generales_Cliente20[i].InnerText;
                            Generales_Cliente[z, 20] = Generales_Cliente21[i].InnerText;
                            Generales_Cliente[z, 21] = Generales_Cliente22[i].InnerText;
                            Generales_Cliente[z, 22] = Generales_Cliente23[i].InnerText;
                            Generales_Cliente[z, 23] = Generales_Cliente24[i].InnerText;
                            Generales_Cliente[z, 24] = Generales_Cliente25[i].InnerText;
                            Generales_Cliente[z, 25] = Generales_Cliente26[i].InnerText;
                            Generales_Cliente[z, 26] = Generales_Cliente27[i].InnerText;
                            Generales_Cliente[z, 27] = Generales_Cliente28[i].InnerText;
                            Generales_Cliente[z, 28] = Generales_Cliente29[i].InnerText;
                            Generales_Cliente[z, 29] = Generales_Cliente30[i].InnerText;
                            Generales_Cliente[z, 30] = Generales_Cliente31[i].InnerText;
                            Generales_Cliente[z, 31] = Generales_Cliente32[i].InnerText;
                            Generales_Cliente[z, 32] = Generales_Cliente33[i].InnerText;
                            Generales_Cliente[z, 33] = Generales_Cliente34[i].InnerText;
                            Generales_Cliente[z, 34] = Generales_Cliente35[i].InnerText;
                            Generales_Cliente[z, 35] = Generales_Cliente36[i].InnerText;
                            Generales_Cliente[z, 36] = Generales_Cliente37[i].InnerText;
                            Generales_Cliente[z, 37] = Generales_Cliente38[i].InnerText;
                            Generales_Cliente[z, 38] = Generales_Cliente39[i].InnerText;

                            string fechadia;

                            fechadia = DateTime.Now.ToString("yyyyMMdd");

                            int x1, x2, x3;
                            decimal y1;
                            int metodopago;
                            string v1 = "";

                            if (Generales_Cliente[z, 15] != "") { y1 = Convert.ToDecimal(Generales_Cliente[z, 15]); } else { y1 = 0; } //LIM_CRE
                            if (Generales_Cliente[z, 16] != "") { x1 = Convert.ToInt32(Generales_Cliente[z, 16]); } else { x1 = 0; }  //PLA_CRE
                            if (Generales_Cliente[z, 18] != "") { x2 = Convert.ToInt32(Generales_Cliente[z, 18]); } else { x2 = 1; } //COD_STS
                            if (Generales_Cliente[z, 30] != "") { metodopago = Convert.ToInt32(Generales_Cliente[z, 30]); } else { metodopago = 1; }

                            if (Generales_Cliente[z, 20] != "")
                            {
                                if (BD.consulta("SELECT COUNT(*)FROM tblRutas WHERE COD_RUTA = '" + Generales_Cliente[z, 20] + "'") != " ") { v1 = Generales_Cliente[z, 20]; } else { }
                            }
                            else
                            {
                                escribe(1, "No existe la ruta " + Generales_Cliente[z, 20], nombre);

                            }

                            if (Generales_Cliente[z, 21] != "") { x3 = Convert.ToInt32(Generales_Cliente[z, 21]); } else { x3 = 1; }



                            if (BD.consulta("SELECT COUNT(*) FROM tblCatClientes WHERE COD_CLI ='" + Generales_Cliente[z, 0] + "'") == "0")
                            {
                                BD.FunicionEjecucion("INSERT INTO tblCatClientes(COD_CLI, NOM_CLI, CALL_NUM, COL_CLI, COP_CLI, COD_CID, COD_EST, COD_PAIS, TEL1_CLI, TEL2_CLI, FAX_CLI, MAIL_CLI, RFC_CLI, CURP_CLI, GPO_CLI, LIM_CRE, PLA_CRE, POR_DES, COD_STS, ARCHIVO, COD_RUTA, COD_LISTA, CONTACTO1, PUESTO1, CONTACTO2, PUESTO2, CONTACTO3, PUESTO3, CONTACTO4, PUESTO4, FEC_ING, FEC_UVEN, FEC_UPAG, FEC_LCRE , SAL_CLI, METODO_PAGO, CUENTA_PAGO, NUM_EXT, NUM_INT, MAIL_CFD, NOM_COM, DP_CLI, RIF_CFDI, ID_CTAPAGO) VALUES ('" + Generales_Cliente[z, 0] + "', '" + Generales_Cliente[z, 1] + "', '" + Generales_Cliente[z, 2] + "', '" + Generales_Cliente[z, 3] + "', '" + Generales_Cliente[z, 4] + "', '" + Generales_Cliente[z, 5] + "', '" + Generales_Cliente[z, 6] + "',  '" + Generales_Cliente[z, 7] + "', '" + Generales_Cliente[z, 8] + "', '" + Generales_Cliente[z, 9] + "', '" + Generales_Cliente[z, 10] + "', '" + Generales_Cliente[z, 11] + "', '" + Generales_Cliente[z, 12] + "', '" + Generales_Cliente[z, 13] + "', '" + Generales_Cliente[z, 14] + "', " + y1 + ", " + x1 + ", " + Convert.ToDouble(Generales_Cliente[z, 17]) + ", " + x2 + ", '" + Generales_Cliente[z, 19] + "', '" + v1 + "' , " + x3 + ", '" + Generales_Cliente[z, 22] + "', '" + Generales_Cliente[z, 23] + "', '" + Generales_Cliente[z, 24] + "', '" + Generales_Cliente[z, 25] + "', '" + Generales_Cliente[z, 26] + "', '" + Generales_Cliente[z, 27] + "', '" + Generales_Cliente[z, 28] + "', '" + Generales_Cliente[z, 29] + "', '" + fechadia + "', '" + fechadia + "', '" + fechadia + "', '" + fechadia + "', 0, " + metodopago + ", '" + Generales_Cliente[z, 31] + "', '" + Generales_Cliente[z, 32] + "', '" + Generales_Cliente[z, 33] + "', '" + Generales_Cliente[z, 34] + "', '" + Generales_Cliente[z, 35] + "', '" + Generales_Cliente[z, 36] + "', '" + Generales_Cliente[z, 37] + "',0)");//** 8.5.0 .. 2018-07-18 AGREGUÉ ID_CTAPAGO CON VALOR 0
                            }
                            else
                            {
                                string creditos = "";
                                if (Generales_Cliente[z, 15] != "") { creditos = ", LIM_CRE =" + y1; } //LIM_CRE
                                if (Generales_Cliente[z, 16] != "") { creditos = creditos + " , PLA_CRE =" + x1; }  //PLA_CRE
                                if (Generales_Cliente[z, 18] != "") { creditos = creditos + " , COD_STS =" + x2; } //COD_STS


                                BD.FunicionEjecucion("UPDATE tblCatClientes SET NOM_CLI = '" + Generales_Cliente[z, 1] + "', CALL_NUM = '" + Generales_Cliente[z, 2] + "', COL_CLI = '" + Generales_Cliente[z, 3] + "', COP_CLI = '" + Generales_Cliente[z, 4] + "', COD_CID ='" + Generales_Cliente[z, 5] + "', COD_EST ='" + Generales_Cliente[z, 6] + "', COD_PAIS ='" + Generales_Cliente[z, 7] + "', TEL1_CLI ='" + Generales_Cliente[z, 8] + "', TEL2_CLI ='" + Generales_Cliente[z, 9] + "', FAX_CLI ='" + Generales_Cliente[z, 10] + "', MAIL_CLI ='" + Generales_Cliente[z, 11] + "', RFC_CLI ='" + Generales_Cliente[z, 12] + "', CURP_CLI ='" + Generales_Cliente[z, 13] + "', GPO_CLI ='" + Generales_Cliente[z, 14] + "', POR_DES =" + Convert.ToDouble(Generales_Cliente[z, 17]) + ", ARCHIVO ='" + Generales_Cliente[z, 19] + "', COD_RUTA ='" + v1 + "' , COD_LISTA =" + x3 + ", CONTACTO1 ='" + Generales_Cliente[z, 22] + "', PUESTO1 ='" + Generales_Cliente[z, 23] + "', CONTACTO2 ='" + Generales_Cliente[z, 24] + "', PUESTO2 ='" + Generales_Cliente[z, 25] + "', CONTACTO3 ='" + Generales_Cliente[z, 26] + "', PUESTO3 ='" + Generales_Cliente[z, 27] + "', CONTACTO4 ='" + Generales_Cliente[z, 28] + "', PUESTO4 ='" + Generales_Cliente[z, 29] + "', METODO_PAGO=" + metodopago + " ,CUENTA_PAGO='" + Generales_Cliente[z, 31] + "', NOM_COM='" + Generales_Cliente[z, 35] + "', DP_CLI= '" + Generales_Cliente[z, 36] + "', RIF_CFDI='" + Generales_Cliente[z, 37] + "' " + creditos + " WHERE COD_CLI ='" + Generales_Cliente[z, 0] + "'");
                            }


                        }
                    }
                    #endregion

                    #region tblEncDevoluciones
                    escribe(1, "EncDevoluciones", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblEncDevoluciones");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Datos_Generales_DC");
                        #region Datos_Generales_NV
                        foreach (XmlElement nodo2 in listaN1)
                        {

                            int i = 0;

                            XmlNodeList tipo_de_movimiento = nodo2.GetElementsByTagName("tipo_de_movimiento");
                            XmlNodeList folioDC = nodo2.GetElementsByTagName("folioDC");
                            XmlNodeList conceptoDC = nodo2.GetElementsByTagName("conceptoDC");
                            XmlNodeList folioconceptoDC = nodo2.GetElementsByTagName("folioconceptoDC");
                            XmlNodeList conceptpgralDC = nodo2.GetElementsByTagName("conceptogralDC");
                            XmlNodeList folgralconceptoDC = nodo2.GetElementsByTagName("folgralconceptoDC");
                            XmlNodeList tipoDC = nodo2.GetElementsByTagName("tipoDC");
                            XmlNodeList referenciaDC = nodo2.GetElementsByTagName("referenciaDC");
                            XmlNodeList facturaDC = nodo2.GetElementsByTagName("facturaDC");
                            XmlNodeList clienteDC = nodo2.GetElementsByTagName("clienteDC");
                            XmlNodeList fechaDC = nodo2.GetElementsByTagName("fechaDC");
                            XmlNodeList fecharegistroDC = nodo2.GetElementsByTagName("fecharegistroDC");
                            XmlNodeList horaDC = nodo2.GetElementsByTagName("horaDC");
                            XmlNodeList subtotalDC = nodo2.GetElementsByTagName("subtotalDC");
                            XmlNodeList impuestoDC = nodo2.GetElementsByTagName("impuestoDC");
                            XmlNodeList importe_exentoDC = nodo2.GetElementsByTagName("importe_exentoDC");
                            XmlNodeList totalDC = nodo2.GetElementsByTagName("totalDC");
                            XmlNodeList regcargoDC = nodo2.GetElementsByTagName("regcargoDC");
                            XmlNodeList impuesto_integradoDC = nodo2.GetElementsByTagName("impuesto_integradoDC");
                            XmlNodeList almacenDC = nodo2.GetElementsByTagName("almacenDC");
                            XmlNodeList estatusDC = nodo2.GetElementsByTagName("estatusDC");
                            XmlNodeList notasDC = nodo2.GetElementsByTagName("notasDC");
                            XmlNodeList usuarioDC = nodo2.GetElementsByTagName("usuarioDC");
                            XmlNodeList sucursalDC = nodo2.GetElementsByTagName("sucursalDC");
                            XmlNodeList contabilizadaDC = nodo2.GetElementsByTagName("contabilizadaDC");
                            XmlNodeList conf_cfdi = nodo2.GetElementsByTagName("conf_cfdi");


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

                            string usuarioNV1 = "";
                            if (dtUsuarios.Select("COD_USU = '" + DatosNotaDeDevolucion[0, 22] + "'").Length == 0)
                            {
                                usuarioNV1 = "DEPURADO"; escribe(3, "No existe el usuario " + DatosNotaDeDevolucion[0, 22], nombre);
                            }
                            else
                            {
                                usuarioNV1 = DatosNotaDeDevolucion[0, 22];
                            }

                            string almacenN = "";
                            if (dtCatAlmacenes.Select("COD_ALM = '" + DatosNotaDeDevolucion[0, 19] + "'").Length == 0)
                            {
                                almacenN = BD.consulta("SELECT COD_ALM FROM tblcatalmacenes WHERE COD_TIP=1;"); //ConfigurationSettings.AppSettings["Almacen"].ToString();
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

                            if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosNotaDeDevolucion[0, 9] + "'") != "0")
                            {
                                if (BD.consulta("SELECT COUNT(FOL_DEV) FROM tblEncDevolucion WHERE  FOL_DEV ='" + DatosNotaDeDevolucion[0, 1] + "'") == "0")
                                {
                                    //                                                1        2        3        4         5        6       7        8           9      10       11         12          13    14          15          16          17          18      19      20       21     22      23          24
                                    BD.GuardaCambios("INSERT INTO tblEncDevolucion(FOL_DEV, COD_CON, FOL_CON, CON_GRL, FOL_GRL, TIP_DEV, REF_DOC, FOLIO_FACT, COD_CLI, FEC_DEV, FEC_REG, HORA_REG, SUB_DEV, IVA_DEV, IMPTO_IMPTOT, TOT_DEV, REG_CARGO, IMPTO_INT, COD_ALM, COD_STS, NOTA, COD_USU, COD_SUCU, CONTAB, CONF_CFDI, ENVIADO) VALUES "
                                       + " ('" + DatosNotaDeDevolucion[0, 1] + "', '" + DatosNotaDeDevolucion[0, 2] + "', '" + DatosNotaDeDevolucion[0, 3] + "', '" + DatosNotaDeDevolucion[0, 4] + "', '" + DatosNotaDeDevolucion[0, 5] + "', " + DatosNotaDeDevolucion[0, 6] + ", '" + DatosNotaDeDevolucion[0, 7] + "', '" + DatosNotaDeDevolucion[0, 8] + "', '" + DatosNotaDeDevolucion[0, 9] + "', '" + fecha1 + "', '" + fecha2 + "', '" + DatosNotaDeDevolucion[0, 12] + "', " + DatosNotaDeDevolucion[0, 13] + ", " + DatosNotaDeDevolucion[0, 14] + ", " + DatosNotaDeDevolucion[0, 15] + ", " + DatosNotaDeDevolucion[0, 16] + ", " + DatosNotaDeDevolucion[0, 17] + ", " + DatosNotaDeDevolucion[0, 18] + ",  '" + DatosNotaDeDevolucion[0, 19] + "', " + DatosNotaDeDevolucion[0, 20] + ", '" + DatosNotaDeDevolucion[0, 21] + "', '" + DatosNotaDeDevolucion[0, 22] + "', '" + DatosNotaDeDevolucion[0, 23] + "', " + DatosNotaDeDevolucion[0, 24] + ", '" + DatosNotaDeDevolucion[0, 25] + "', 0);");


                                }
                                else
                                {
                                    escribe(3, "Ya existe la Nota de Venta REF_DOC " + DatosNotaDeDevolucion[0, 1], nombre);
                                }
                            }
                            else
                            {
                                escribe(3, "No existe el cliente " + DatosNotaDeDevolucion[0, 9], nombre);

                            }

                        }
                        #endregion
                    }
                    #endregion

                    #region tblRenDevoluciones
                    escribe(1, "RenDevoluciones", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblRenDevoluciones");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        #region Partidas_DC
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Partidas_DC");
                        foreach (XmlElement nodo2 in listaN1)
                        {

                            int i = 0;

                            XmlNodeList folioRENDC = nodo2.GetElementsByTagName("folioRENDC");
                            XmlNodeList foliogeneralRENDC = nodo2.GetElementsByTagName("foliogeneralRENDC");
                            XmlNodeList cantidadRENDC = nodo2.GetElementsByTagName("cantidadRENDC");
                            XmlNodeList articuloRENDC = nodo2.GetElementsByTagName("articuloRENDC");
                            XmlNodeList sustituidoRENDC = nodo2.GetElementsByTagName("sustituidoRENDC");
                            XmlNodeList unidadRENDC = nodo2.GetElementsByTagName("unidadRENDC");
                            XmlNodeList precio_ventaRENDC = nodo2.GetElementsByTagName("precio_ventaRENDC");
                            XmlNodeList equivalenciaRENDC = nodo2.GetElementsByTagName("equivalenciaRENDC");
                            XmlNodeList costo_de_ventaRENDC = nodo2.GetElementsByTagName("costo_de_ventaRENDC");
                            XmlNodeList movimientoRENDC = nodo2.GetElementsByTagName("movimientoRENDC");
                            XmlNodeList importe_exentoRENDC = nodo2.GetElementsByTagName("importe_exentoRENDC");
                            XmlNodeList precio_netoRENDC = nodo2.GetElementsByTagName("precio_netoRENDC");
                            XmlNodeList codigo_impto1RENDC = nodo2.GetElementsByTagName("codigo_impto1RENDC");
                            XmlNodeList codigo_impto2RENDC = nodo2.GetElementsByTagName("codigo_impto2RENDC");
                            XmlNodeList porcentaje_impto1RENDC = nodo2.GetElementsByTagName("porcentaje_impto1RENDC");
                            XmlNodeList porcentaje_impto2RENDC = nodo2.GetElementsByTagName("porcentaje_impto2RENDC");
                            XmlNodeList importe_impto1RENDC = nodo2.GetElementsByTagName("importe_impto1RENDC");
                            XmlNodeList importe_impto2RENDC = nodo2.GetElementsByTagName("importe_impto2RENDC");

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

                            if (BD.consulta("SELECT COUNT(*) FROM tblEncDevolucion WHERE FOL_DEV ='" + RengloNotaDeDevolucion[0, 0] + "'") != "0")
                            {
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
                                BD.GuardaCambios("INSERT INTO tblRenDevolucion(FOL_DEV, FOL_GRL, CAN_DEV, COD1_ART, SUS1_ART, COD_UND, PCIO_ART, EQV_UND, COS_ART, NUM_MOV, IMPTO_IMP, PCIO_NETO, COD1_IMP, COD2_IMP, IMP1_ART, IMP2_ART, IMP1_REG, IMP2_REG) VALUES " +
                                    " ('" + RengloNotaDeDevolucion[0, 0] + "', '" + RengloNotaDeDevolucion[0, 1] + "', " + RengloNotaDeDevolucion[0, 2] + ", '" + articuloD + "', '" + articuloS + "', '" + unidadN + "', " + RengloNotaDeDevolucion[0, 6] + ", " + RengloNotaDeDevolucion[0, 7] + ", " + RengloNotaDeDevolucion[0, 8] + ", " + RengloNotaDeDevolucion[0, 9] + ", " + RengloNotaDeDevolucion[0, 10] + ", " + RengloNotaDeDevolucion[0, 11] + ", " + RengloNotaDeDevolucion[0, 12] + ", " + RengloNotaDeDevolucion[0, 13] + ", " + RengloNotaDeDevolucion[0, 14] + ", " + RengloNotaDeDevolucion[0, 15] + ", " + RengloNotaDeDevolucion[0, 16] + ", " + RengloNotaDeDevolucion[0, 17] + ");");//Corregido 15-abr-2013.. Tenía comilla al final y no la tenía en el z, 0

                                #region
                                string folio = "";
                                folio = BD.consulta("SELECT REF_DOC FROM tblEncDevolucion WHERE FOL_DEV = '" + RengloNotaDeDevolucion[0, 0] + "';");
                                if (BD.consulta("SELECT COUNT(REF_DOC)  FROM tblGralVentas WHERE REF_DOC = '" + folio + "';") != "0")
                                {
                                    BD.GuardaCambios("UPDATE tblRenventas SET CAN_DEV=CAN_DEV + " + RengloNotaDeDevolucion[0, 2] + " WHERE REF_DOC='" + folio + "' AND NUM_MOV=" + RengloNotaDeDevolucion[0, 9] + ";");
                                }
                                else
                                {
                                    escribe(3, "No existe la nota de venta " + folio, nombre);
                                }
                                #endregion
                            }
                            else
                            {
                                escribe(3, "No existe el encabezado de devolucion " + RengloNotaDeDevolucion[0, 0], nombre);

                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region EncCargosAbonos

                    escribe(1, "EncCargosAbonos", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblEncCargosAbonos");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Cartera");
                        foreach (XmlElement nodo2 in listaN1)
                        {

                            XmlNodeList tipo_de_movimiento = nodo2.GetElementsByTagName("tipo_de_movimiento");
                            XmlNodeList folio = nodo2.GetElementsByTagName("folio");
                            XmlNodeList folio_general = nodo2.GetElementsByTagName("folio_general");
                            XmlNodeList concepto = nodo2.GetElementsByTagName("concepto");
                            XmlNodeList concepto_del_documento = nodo2.GetElementsByTagName("concepto_del_documento");
                            XmlNodeList concepto_general = nodo2.GetElementsByTagName("concepto_general");
                            XmlNodeList clientes = nodo2.GetElementsByTagName("clientes");
                            XmlNodeList fecha = nodo2.GetElementsByTagName("fecha");
                            XmlNodeList hora = nodo2.GetElementsByTagName("hora");
                            XmlNodeList fecha_registro = nodo2.GetElementsByTagName("fecha_registro");
                            XmlNodeList usuario = nodo2.GetElementsByTagName("usuario");
                            XmlNodeList estatus = nodo2.GetElementsByTagName("estatus");
                            XmlNodeList notas = nodo2.GetElementsByTagName("notas");
                            XmlNodeList importe = nodo2.GetElementsByTagName("importe");
                            XmlNodeList porcentaje_impuesto = nodo2.GetElementsByTagName("porcentaje_impuesto");
                            XmlNodeList importe_impuesto = nodo2.GetElementsByTagName("importe_impuesto");
                            XmlNodeList saldo = nodo2.GetElementsByTagName("saldo");
                            XmlNodeList plazo = nodo2.GetElementsByTagName("plazo");
                            XmlNodeList caja = nodo2.GetElementsByTagName("caja");
                            XmlNodeList sucursal = nodo2.GetElementsByTagName("sucursal");
                            XmlNodeList contabilizado = nodo2.GetElementsByTagName("contabilizado");
                            XmlNodeList folio_liq = nodo2.GetElementsByTagName("folio_liq");
                            XmlNodeList CONF_CFDI = nodo2.GetElementsByTagName("CONF_CFDI");
                            XmlNodeList FOL_FACT = nodo2.GetElementsByTagName("FOL_FACT");


                            string conceptoDF = "";
                            if (BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE COD_CON ='" + concepto[0].InnerText + "'") == "0")
                            {

                                //CarteraConcepto
                                XmlNodeList listaN2 = nodo1.GetElementsByTagName("CarteraConcepto");
                                foreach (XmlElement nodo3 in listaN2)
                                {
                                    XmlNodeList DES_CON = nodo3.GetElementsByTagName("DES_CON");
                                    XmlNodeList FOL_CON = nodo3.GetElementsByTagName("FOL_CON");
                                    XmlNodeList TIP_MOV = nodo3.GetElementsByTagName("TIP_MOV");
                                    XmlNodeList FOL_EXTRA = nodo3.GetElementsByTagName("FOL_EXTRA");
                                    XmlNodeList AGOTADO = nodo3.GetElementsByTagName("AGOTADO");
                                    XmlNodeList CFD = nodo3.GetElementsByTagName("CFD");
                                    XmlNodeList AP_ABONO = nodo3.GetElementsByTagName("AP_ABONO");
                                    XmlNodeList CFDI_PAGO = nodo3.GetElementsByTagName("CFDI_PAGO");
                                    XmlNodeList CON_AJUSTE = nodo3.GetElementsByTagName("CON_AJUSTE");
                                    XmlNodeList ANTIB_USO = nodo3.GetElementsByTagName("ANTIB_USO");

                                    string sentencia = "INSERT INTO tblconceptos (COD_CON, DES_CON, FOL_CON, TIP_MOV, FOL_EXTRA, AGOTADO, CFD, AP_ABONO, CFDI_PAGO, CON_AJUSTE, ANTIB_USO) VALUES " +
                                                       "('" + concepto[0].InnerText + "', '" + DES_CON[0].InnerText + "', '" + FOL_CON[0].InnerText + "', '" + TIP_MOV[0].InnerText + "', '" + FOL_EXTRA[0].InnerText + "', " + AGOTADO[0].InnerText + ", " + CFD[0].InnerText + ", " + AP_ABONO[0].InnerText + ", " + CFDI_PAGO[0].InnerText + ", " + CON_AJUSTE[0].InnerText + ", " + ANTIB_USO[0].InnerText + ");";

                                    if (BD.FunicionEjecucion(sentencia))
                                    {
                                        conceptoDF = concepto_del_documento[0].InnerText;
                                        escribe(3, "Se pudo agregar el concepto " + concepto[0].InnerText + " Desc: " + DES_CON[0].InnerText, nombre);

                                    }
                                    else
                                    {
                                        conceptoDF = (BD.consulta("SELECT COD_CON FROM tblconceptos WHERE TIP_MOV ='" + concepto[0].InnerText + "'"));
                                        escribe(3, "No se pudo agregar el concepto " + concepto[0].InnerText, nombre);
                                    }

                                }
                            }
                            else
                            {
                                conceptoDF = concepto_del_documento[0].InnerText;
                            }

                            string usuarioX = "";
                            if (usuario[0].InnerText.Length > 0)
                            {
                                if (dtUsuarios.Select("COD_USU = '" + usuario[0].InnerText + "'").Length == 0)
                                {
                                    usuarioX = "DEPURADO";//“No existe el usuario” + <usuario>  DatosFactura[0, 21], 
                                    escribe(3, "No existe el usuario " + usuario[0].InnerText + " para el folio " + folio[0].InnerText, nombre);
                                }
                                else
                                {
                                    usuarioX = usuario[0].InnerText;
                                }
                            }

                            int cajaX;
                            if (dtCajas.Select("COD_CAJ = " + caja[0].InnerText).Length == 0)
                            {
                                cajaX = Convert.ToInt32(BD.consulta("SELECT COUNT(COD_CAJ) FROM tblCajas WHERE VEN_CAJ = 1 ")); //“No existe la caja de cobranza” + <caja> “se asignó 1
                                escribe(3, "No existe la caja de cobranza " + caja[0].InnerText + " se asignó 1", nombre);
                            }
                            else
                            {
                                cajaX = Convert.ToInt32(caja[0].InnerText);
                            }


                            if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + clientes[0].InnerText + "'") == "1")
                            {
                                #region
                                if (BD.consulta("SELECT COUNT(*) FROM tblEncCargosAbonos WHERE FOL_DOC ='" + folio_general[0].InnerText + "'") != "0")
                                {
                                    string sentencia = "UPDATE tblEncCargosAbonos SET " +
                                    " FOL_DOC='" + folio[0].InnerText + "', COD_CON='" + concepto[0].InnerText + "', CON_CEP='" + conceptoDF + "', " +
                                    " CON_GRL='" + concepto[0].InnerText + "', COD_CLI='" + clientes[0].InnerText + "', FEC_DOC='" + CambioDeFecha(fecha[0].InnerText) + "', " +
                                    " HORA_DOC='" + hora[0].InnerText + "', FEC_REG='" + CambioDeFecha(fecha_registro[0].InnerText) + "', COD_USU='" + usuarioX + "', " +
                                    " COD_STS=" + Convert.ToInt16(estatus[0].InnerText) + ", NOTA='" + notas[0].InnerText + "', IMP_DOC=" + Convert.ToDecimal(importe[0].InnerText) + ", " +
                                    " POR_IMP=" + Convert.ToDecimal(porcentaje_impuesto[0].InnerText) + ", IVA_DOC=" + Convert.ToDecimal(importe_impuesto[0].InnerText) + ", " +
                                    " SAL_DOC=" + Convert.ToDecimal(saldo[0].InnerText) + ", PLA_PAG=" + Convert.ToInt64(plazo[0].InnerText) + ", " +
                                    " COD_CAJ=" + cajaX + ", COD_SUCU='" + sucursal[0].InnerText + "', CONTAB=" + Convert.ToInt16(contabilizado[0].InnerText) + ", " +
                                    " FOL_LIQ='" + folio_liq[0].InnerText + "', ENVIADO=0, CONF_CFDI='" + CONF_CFDI[0].InnerText + "', FOL_FACT='" + FOL_FACT[0].InnerText + "' " +
                                    " WHERE  FOL_GRL = '" + folio_general[0].InnerText + "';";
                                    BD.FunicionEjecucion(sentencia);

                                }
                                else
                                {
                                    string sentencia = "INSERT INTO tblEncCargosAbonos(FOL_DOC, FOL_GRL, COD_CON, CON_CEP, CON_GRL, COD_CLI, FEC_DOC, HORA_DOC, FEC_REG, COD_USU, COD_STS, NOTA, IMP_DOC, POR_IMP, IVA_DOC, SAL_DOC, PLA_PAG, COD_CAJ, COD_SUCU, CONTAB, FOL_LIQ, ENVIADO, CONF_CFDI, FOL_FACT) VALUES " +
                                   "('" + folio[0].InnerText + "', '" + folio_general[0].InnerText + "', '" + concepto[0].InnerText + "', '" + conceptoDF + "', '" + concepto_general[0].InnerText + "', '" + clientes[0].InnerText + "', '" + CambioDeFecha(fecha[0].InnerText) + "', '" + hora[0].InnerText + "', '" + CambioDeFecha(fecha_registro[0].InnerText) + "', '" + usuarioX + "', " + Convert.ToInt16(estatus[0].InnerText) + ", '" + notas[0].InnerText + "', " + Convert.ToDecimal(importe[0].InnerText) + ", " + Convert.ToDecimal(porcentaje_impuesto[0].InnerText) + ", " + Convert.ToDecimal(importe_impuesto[0].InnerText) + ", " + Convert.ToDecimal(saldo[0].InnerText) + ", " + Convert.ToInt64(plazo[0].InnerText) + ", " + cajaX + ", '" + sucursal[0].InnerText + "', " + Convert.ToInt16(contabilizado[0].InnerText) + ", '" + folio_liq[0].InnerText + "', 0, '" + CONF_CFDI[0].InnerText + "', '" + FOL_FACT[0].InnerText + "')";
                                    //MessageBox.Show(sentencia + Environment.NewLine + clienteNV1);
                                    BD.FunicionEjecucion(sentencia);
                                    #region saldo cliente
                                    if (clientes[0].InnerText != "PUBLIC")
                                    {

                                        decimal SaldoTotal;
                                        string datototal;

                                        datototal = BD.consulta("SELECT SAL_CLI FROM tblCatClientes WHERE COD_CLI = '" + clientes[0].InnerText + "'");


                                        SaldoTotal = Convert.ToDecimal(datototal);

                                        if (concepto_general[0].InnerText == "CCLI")
                                        {
                                            SaldoTotal = SaldoTotal + Convert.ToDecimal(importe[0].InnerText);
                                        }
                                        else
                                        {
                                            if (concepto_general[0].InnerText == "ACLI")
                                            {
                                                SaldoTotal = SaldoTotal - Convert.ToDecimal(importe[0].InnerText);
                                            }
                                        }

                                        BD.FunicionEjecucion("UPDATE tblCatClientes SET SAL_CLI = " + SaldoTotal + " WHERE COD_CLI = '" + clientes[0].InnerText + "'");
                                    }
                                    #endregion

                                }

                                #endregion
                            }
                            else
                            {
                                escribe(3, "No existe el cliente " + clientes[0].InnerText + " para el folio " + folio[0].InnerText, nombre);

                            }

                        }
                    }
                    #endregion

                    #region RenCargosAbonos
                    escribe(1, "RenCargosAbonos", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblRenCargosAbonos");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Documentos_Afectados");
                        foreach (XmlElement nodo2 in listaN1)
                        {
                            XmlNodeList folio_carter = nodo2.GetElementsByTagName("folio_carter");
                            XmlNodeList folio_documento = nodo2.GetElementsByTagName("folio_documento");
                            XmlNodeList folio_general = nodo2.GetElementsByTagName("folio_general");
                            XmlNodeList concepto = nodo2.GetElementsByTagName("concepto");
                            XmlNodeList concepto_documento = nodo2.GetElementsByTagName("concepto_documento");
                            XmlNodeList concepto_general = nodo2.GetElementsByTagName("concepto_general");
                            XmlNodeList importe_aplicado = nodo2.GetElementsByTagName("importe_aplicado");
                            XmlNodeList saldo_del_renglon = nodo2.GetElementsByTagName("saldo_del_renglon");
                            XmlNodeList total_del_documento = nodo2.GetElementsByTagName("total_del_documento");
                            XmlNodeList estatus = nodo2.GetElementsByTagName("estatus");
                            XmlNodeList FEC_DOC = nodo2.GetElementsByTagName("FEC_DOC");

                            Int64 qwe; //FOL_GRL
                            if (BD.consulta("SELECT COUNT(*) FROM tblEncCargosAbonos WHERE FOL_DOC ='" + folio_carter[0].InnerText + "'") != "0")
                            {
                                qwe = Convert.ToInt64(BD.consulta("SELECT NUM_MOV FROM tblEncCargosAbonos WHERE FOL_DOC ='" + folio_carter[0].InnerText + "'"));
                                if (BD.consulta("SELECT COUNT(*) FROM tblEncCargosAbonos WHERE FOL_GRL ='" + folio_general[0].InnerText + "'") != "0")
                                {
                                    string cliente = BD.consulta("SELECT COD_CLI FROM tblEncCargosAbonos WHERE FOL_DOC ='" + folio_carter[0].InnerText + "'");
                                    string fecha = BD.consulta("SELECT  CONVERT(char(10),FEC_REG,126) AS fecha  FROM tblEncCargosAbonos WHERE FOL_DOC ='" + folio_carter[0].InnerText + "'");
                                    string sentencia = "INSERT INTO tblRenCargosAbonos(FOL_DOC, FOL_REF, FOL_GRL, COD_CON, CON_CEP, CON_GRL, IMP_DOC, SAL_DOC, TOT_DOC, COD_STS, FEC_DOC, COD_CLI, NUM_MOV) VALUES ('" + folio_carter[0].InnerText + "', '" + folio_documento[0].InnerText + "', '" + folio_general[0].InnerText + "', '" + concepto[0].InnerText + "', '" + concepto_documento[0].InnerText + "', '" + concepto_general[0].InnerText + "', " + Convert.ToDecimal(importe_aplicado[0].InnerText) + ", " + Convert.ToDecimal(saldo_del_renglon[0].InnerText) + ", " + Convert.ToDecimal(total_del_documento[0].InnerText) + ", " + Convert.ToInt16(estatus[0].InnerText) + ", '" + FEC_DOC[0].InnerText + "', '" + cliente + "', " + qwe + ")";
                                    BD.FunicionEjecucion(sentencia);


                                    //Saldos
                                    XmlNodeList listaN2 = nodo2.GetElementsByTagName("Saldo_Documento");
                                    foreach (XmlElement nodo3 in listaN2)
                                    {
                                        XmlNodeList SAL_DOC = nodo3.GetElementsByTagName("SAL_DOC");
                                        //Sustituye Saldo_Documento(SAL_DOC) en el encablezado del documento afectado 
                                        //tblEncCargosAbono WHERE FOL_DOC = FOL_REF
                                        sentencia = " UPDATE tblEncCargosAbonos SET SAL_DOC = " + SAL_DOC[0].InnerText + "  WHERE FOL_DOC = '" + folio_documento[0].InnerText + "'; ";
                                        BD.FunicionEjecucion(sentencia);
                                        sentencia = " UPDATE tblRenCargosAbonos SET SAL_DOC = " + SAL_DOC[0].InnerText + "  WHERE FOL_DOC = '" + folio_documento[0].InnerText + "'; ";
                                        BD.FunicionEjecucion(sentencia);

                                    }






                                }
                            }
                            else
                            {
                                escribe(3, "No existe el encabezado de movimiento  " + folio_carter[0].InnerText, nombre);

                            }
                        }
                    }
                    #endregion


                    #region  FacturasEnc
                    escribe(1, "FacturasEnc", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblFacturasEnc");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Datos_Generales_Factura");
                        foreach (XmlElement nodo2 in listaN1)
                        {
                            int i = 0;

                            XmlNodeList tipo_de_movimiento = nodo2.GetElementsByTagName("tipo_de_movimiento");
                            XmlNodeList foliofact = nodo2.GetElementsByTagName("foliofact");
                            XmlNodeList folio_general = nodo2.GetElementsByTagName("folio_general");
                            XmlNodeList concepto = nodo2.GetElementsByTagName("concepto");
                            XmlNodeList clientefact = nodo2.GetElementsByTagName("clientefact");
                            XmlNodeList fechafact = nodo2.GetElementsByTagName("fechafact");
                            XmlNodeList subtotalfact = nodo2.GetElementsByTagName("subtotalfact");
                            XmlNodeList impuestofact = nodo2.GetElementsByTagName("impuestofact");
                            XmlNodeList importe_exentofact = nodo2.GetElementsByTagName("importe_exentofact");
                            XmlNodeList totalfact = nodo2.GetElementsByTagName("totalfact");
                            XmlNodeList impuesto_integrado_fact = nodo2.GetElementsByTagName("impuesto_integrado_fact");
                            XmlNodeList tip_fact = nodo2.GetElementsByTagName("tip_fact");
                            XmlNodeList estatusfact = nodo2.GetElementsByTagName("estatusfact");
                            XmlNodeList notasfact = nodo2.GetElementsByTagName("notasfact");
                            XmlNodeList usuariofact = nodo2.GetElementsByTagName("usuariofact");
                            XmlNodeList sucursalfact = nodo2.GetElementsByTagName("sucursalfact");
                            XmlNodeList descuentofact = nodo2.GetElementsByTagName("descuentofact");
                            XmlNodeList cargosfact = nodo2.GetElementsByTagName("cargosfact");
                            XmlNodeList vencimientofact = nodo2.GetElementsByTagName("vencimientofact");
                            XmlNodeList creditofact = nodo2.GetElementsByTagName("creditofact");
                            XmlNodeList importe_creditofact = nodo2.GetElementsByTagName("importe_creditofact");
                            XmlNodeList empresafact = nodo2.GetElementsByTagName("empresafact");
                            XmlNodeList USO_CFDI = nodo2.GetElementsByTagName("USO_CFDI");
                            XmlNodeList CONF_CFDI = nodo2.GetElementsByTagName("CONF_CFDI");
                            XmlNodeList HORA_FAC = nodo2.GetElementsByTagName("HORA_FAC");

                            //sbandera = "Factura datos generales ---  " + foliofact[i].InnerText;
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


                            string usuarioNV123;
                            if (dtUsuarios.Select("COD_USU = '" + DatosFactura[0, 14] + "'").Length == 0)
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



                            string clienteNV123;
                            bool existeCliente = true;
                            if (DatosFactura[0, 4] != "PUBLIC")
                            {
                                if (BD.consulta("SELECT COUNT(COD_CLI) FROM tblCatClientes WHERE COD_CLI ='" + DatosFactura[0, 4] + "'") == "0")
                                {
                                    clienteNV123 = "PUBLIC";//“No existe el cliente” + <clienteNV> + “para la Factura” + <foliofact> DatosFactura[0, 3], 
                                    escribe(3, "No existe el cliente " + DatosFactura[0, 4] + " para la Factura " + DatosFactura[0, 1], nombre);
                                    existeCliente = false;
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

                            if (existeCliente)
                            {
                                #region Inserta
                                if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOLIO_FAC ='" + DatosFactura[0, 1] + "'") != "0")
                                {
                                    escribe(3, "Ya existe la Factura con folio " + DatosFactura[0, 1], nombre);
                                }
                                else
                                {
                                    if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOL_GRL ='" + DatosFactura[0, 2] + "'") != "0")
                                    {
                                        escribe(3, "Ya existe la factura con folio general" + DatosFactura[0, 2], nombre);
                                    }
                                    else
                                    {
                                        //escribe(3, DatosFactura[0, 5] + " --- " + DatosFactura[0, 6] + " --- " + DatosFactura[0, 7] + " --- " + DatosFactura[0, 8] + " --- " + DatosFactura[0, 9] + " --- " + DatosFactura[0, 10] + " --- " + DatosFactura[0, 11] + " --- " + DatosFactura[0, 12] + " --- " + DatosFactura[0, 15] + " --- " + DatosFactura[0, 16] + " --- " + DatosFactura[0, 17] + " --- " + DatosFactura[0, 18] + " --- " + DatosFactura[0, 20], nombre);

                                        string f1 = DatosFactura[0, 18];


                                        f1 = CambioDeFecha(DatosFactura[0, 18]);

                                        string sent = "INSERT INTO tblfacturasenc(FOLIO_FAC, FOL_GRL, COD_CON, COD_CLI, FEC_FAC, SUB_DOC, IVA_DOC, IMPTO_IMPTOT, TOT_DOC, IMPTO_INT, TOTAL_TIP, STS_DOC, NOTA, COD_USU, COD_SUCU, DES_CLI, CAR1_VEN, FEC_VENC, CREDITO, IMPORTE_CRED, COD_EMPRESA, ENVIADO, HORA_FAC, FOLIO_DIG, SAT_MPAGO, CTA_PAGO, DP_DESTINO, DP_ENTREGA, USO_CFDI, CONF_CFDI) VALUES ('" +
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
                                        empresaNV1234 + ", 0, '" + DatosFactura[0, 26] + "', 'S/N', '', '', '" + DatosFactura[0, 22] + "', " + DatosFactura[0, 23] + ", '" + DatosFactura[0, 24] + "', '" + DatosFactura[0, 25] + "')";
                                        //escribe(1, sent, nombre);
                                        BD.FunicionEjecucion(sent);


                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion

                    #region FacturasRen
                    escribe(1, "FacturasRen", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblFacturasRen");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Partidas_Factura");
                        foreach (XmlElement nodo2 in listaN1)
                        {
                            int i = 0;

                            XmlNodeList folio = nodo2.GetElementsByTagName("folio");
                            XmlNodeList articulo = nodo2.GetElementsByTagName("articulo");
                            XmlNodeList cantidad = nodo2.GetElementsByTagName("cantidad");
                            XmlNodeList unidad = nodo2.GetElementsByTagName("unidad");
                            XmlNodeList equivalencia = nodo2.GetElementsByTagName("equivalencia");
                            XmlNodeList precio_venta = nodo2.GetElementsByTagName("precio_venta");
                            XmlNodeList moneda = nodo2.GetElementsByTagName("moneda");
                            XmlNodeList tipo_de_cambio = nodo2.GetElementsByTagName("tipo_de_cambio");
                            XmlNodeList porcentaje_descto = nodo2.GetElementsByTagName("porcentaje_descto");
                            XmlNodeList descto_adicional = nodo2.GetElementsByTagName("descto_adicional");
                            XmlNodeList codigo_impto1 = nodo2.GetElementsByTagName("codigo_impto1");
                            XmlNodeList codigo_impto2 = nodo2.GetElementsByTagName("codigo_impto2");
                            XmlNodeList porcentaje_impto1 = nodo2.GetElementsByTagName("porcentaje_impto1");
                            XmlNodeList porcentaje_impto2 = nodo2.GetElementsByTagName("porcentaje_impto2");
                            XmlNodeList importe_impto1 = nodo2.GetElementsByTagName("importe_impto1");
                            XmlNodeList importe_impto2 = nodo2.GetElementsByTagName("importe_impto2");
                            XmlNodeList importe_exento = nodo2.GetElementsByTagName("importe_exento");
                            XmlNodeList numero_movimiento = nodo2.GetElementsByTagName("numero_movimiento");
                            XmlNodeList importe_sindescuento = nodo2.GetElementsByTagName("importe_sindescuento");//19
                            XmlNodeList descuento_general = nodo2.GetElementsByTagName("descuento_general");//20

                            XmlNodeList precio_uni = nodo2.GetElementsByTagName("precio_uni");//21
                            XmlNodeList fecha_cad = nodo2.GetElementsByTagName("fecha_cad");//22
                            XmlNodeList numero_lot = nodo2.GetElementsByTagName("numero_lot");//23
                            XmlNodeList UND_CFDI = nodo2.GetElementsByTagName("UND_CFDI");//24
                            XmlNodeList FOLIO_NV = nodo2.GetElementsByTagName("FOLIO_NV");//25
                            XmlNodeList CVEART_CFDI = nodo2.GetElementsByTagName("CVEART_CFDI");//26 8.1.0 .. 2017-08-14

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

                            if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOLIO_FAC ='" + DatosFactura1[0, 0] + "'") == "0")
                            {
                                escribe(3, "No existe la Factura con folio " + DatosFactura1[0, 0], nombre);
                            }
                            else
                            {
                                #region
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
                                if (dtMoneda.Select("COD_MON = " + Convert.ToInt32(DatosFactura1[0, 6])).Length == 0)
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
                                if (dtImpuestos.Select("COD_IMP = " + Convert.ToInt32(DatosFactura1[0, 10])).Length == 0)
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
                                if (dtImpuestos.Select("COD_IMP = " + Convert.ToInt32(DatosFactura1[0, 11])).Length == 0)
                                {
                                    codigo_impto2N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[0, 11]
                                    escribe(3, "No existe el impuesto " + DatosFactura1[0, 11], nombre);

                                }
                                else
                                {
                                    codigo_impto2N = Convert.ToInt32(DatosFactura1[0, 11]);
                                }

                                string fechacad = CambioDeFecha(DatosFactura1[0, 21]);
                                BD.FunicionEjecucion("INSERT INTO tblFacturasRen (FOLIO_FAC, COD1_ART, CAN_ART, COD_UND, EQV_UND, PCIO_VEN, COD_MON, TIP_CAM, POR_DES, DECTO_ADI, COD1_IMP, COD2_IMP, IMP1_ART, IMP2_ART, IMP1_REG, IMP2_REG, IMPTO_IMP, NUM_MOV, IMP_SINDESC, DCTO_GRAL, PCIO_UNI, NUM_LOT, FEC_CAD, UND_CFDI, FOLIO_NV, CVEART_CFDI, IMPINT_REN) VALUES ('" + DatosFactura1[0, 0] + "', '" + articuloN + "', " + Convert.ToDecimal(DatosFactura1[0, 2]) + ", '" + unidadN + "', " + Convert.ToDecimal(DatosFactura1[0, 4]) + ",  " + verificaLongitud(DatosFactura1[0, 5]) + ", " + monedaN + ", " + Convert.ToDouble(DatosFactura1[0, 7]) + ", " + Convert.ToDecimal(DatosFactura1[0, 8]) + ", " + Convert.ToDecimal(DatosFactura1[0, 9]) + ", " + codigo_impto1N + ", " + codigo_impto2N + ", " + Convert.ToDecimal(DatosFactura1[0, 12]) + ", " + Convert.ToDecimal(DatosFactura1[0, 13]) + ", " + verificaLongitud(DatosFactura1[0, 14]) + ", " + verificaLongitud(DatosFactura1[0, 15]) + ", " + verificaLongitud(DatosFactura1[0, 16]) + ", " + Convert.ToInt64(DatosFactura1[0, 17]) + ", " + verificaLongitud(DatosFactura1[0, 18]) + ", " + verificaLongitud(DatosFactura1[0, 19]) + ", " + verificaLongitud(DatosFactura1[0, 20]) + ", '" + DatosFactura1[0, 22] + "', '" + fechacad + "', '" + DatosFactura1[0, 23] + "', '" + DatosFactura1[0, 24] + "', '" + DatosFactura1[0, 25] + "', " + DatosFactura1[0, 26] + ")");
                                #endregion
                            }
                        }
                    }
                    #endregion

                    #region NotasPorFactura
                    escribe(1, "NotasPorFactura", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblNotasPorFactura");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Folio_NV");
                        foreach (XmlElement nodo2 in listaN1)
                        {
                            int i = 0;

                            XmlNodeList FOLIO_NV = nodo2.GetElementsByTagName("folioNV");
                            XmlNodeList FOLIO_FACT = nodo2.GetElementsByTagName("folioFAC");
                            XmlNodeList TOTAL_NV = nodo2.GetElementsByTagName("totalNV");

                            if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOLIO_FAC ='" + DatosFactura1[0, 0] + "'") == "0")
                            {
                                escribe(3, "No existe la Factura con folio " + FOLIO_NV[i].InnerText, nombre);
                            }
                            else
                            {
                                BD.FunicionEjecucion("INSERT INTO tblNotasPorFactura(FOLIO_NV, FOLIO_FACT, TOTAL_NV) VALUES ('" +
                                       FOLIO_NV[i].InnerText + "',  '" + FOLIO_FACT[i].InnerText + "', " + TOTAL_NV[i].InnerText + ");");
                            }
                        }
                    }
                    #endregion

                    #region FacturaElectronica
                    escribe(1, "FacturaElectronica", nombre);
                    listaT1 = nodo.GetElementsByTagName("tblFacturaElectronica");
                    foreach (XmlElement nodo1 in listaT1)
                    {
                        XmlNodeList listaN1 = nodo1.GetElementsByTagName("Datos");
                        foreach (XmlElement nodo2 in listaN1)
                        {
                            int i = 0;

                            XmlNodeList FOLIO_INTERNO = nodo2.GetElementsByTagName("FOLIO_INTERNO");
                            XmlNodeList SERIE_FISCAL = nodo2.GetElementsByTagName("SERIE_FISCAL");
                            XmlNodeList FOLIO_FISCAL = nodo2.GetElementsByTagName("FOLIO_FISCAL");
                            XmlNodeList NO_APROBACION = nodo2.GetElementsByTagName("NO_APROBACION");
                            XmlNodeList SELLO_FE = nodo2.GetElementsByTagName("SELLO_FE");
                            XmlNodeList CADENA_ORIGINAL = nodo2.GetElementsByTagName("CADENA_ORIGINAL");
                            XmlNodeList ANO_APROBACION = nodo2.GetElementsByTagName("ANO_APROBACION");
                            XmlNodeList TIPO_COMPROBANTE = nodo2.GetElementsByTagName("TIPO_COMPROBANTE");
                            XmlNodeList ESTATUS_FE = nodo2.GetElementsByTagName("ESTATUS_FE");
                            XmlNodeList MENSAJE_FE = nodo2.GetElementsByTagName("MENSAJE_FE");
                            XmlNodeList NO_CERTIFICADO = nodo2.GetElementsByTagName("NO_CERTIFICADO");
                            XmlNodeList TIMBRE_UUID = nodo2.GetElementsByTagName("TIMBRE_UUID");
                            XmlNodeList FECHA_TIMBRADO = nodo2.GetElementsByTagName("FECHA_TIMBRADO");
                            XmlNodeList SELLO_SAT = nodo2.GetElementsByTagName("SELLO_SAT");
                            XmlNodeList CERTIFICADO_SAT = nodo2.GetElementsByTagName("CERTIFICADO_SAT");
                            XmlNodeList REGIMENES = nodo2.GetElementsByTagName("REGIMENES");
                            XmlNodeList COND_PAGO = nodo2.GetElementsByTagName("COND_PAGO");
                            XmlNodeList METODO_PAGO = nodo2.GetElementsByTagName("METODO_PAGO");
                            XmlNodeList NUMCTA_PAGO = nodo2.GetElementsByTagName("NUMCTA_PAGO");
                            XmlNodeList LUGAR_EXPED = nodo2.GetElementsByTagName("LUGAR_EXPED");
                            XmlNodeList TIPO_MONEDA = nodo2.GetElementsByTagName("TIPO_MONEDA");
                            XmlNodeList CONCILIADA = nodo2.GetElementsByTagName("CONCILIADA");
                            XmlNodeList CVEPAGO_SAT = nodo2.GetElementsByTagName("CVEPAGO_SAT");

                            string sentencia = "";
                            if (BD.consulta("SELECT COUNT(*) FROM tblFacturaElectronica WHERE FOLIO_INTERNO ='" + FOLIO_INTERNO[i].InnerText + "'") == "0")
                            {
                                sentencia = "INSERT INTO tblFacturaElectronica (FOLIO_INTERNO ,SERIE_FISCAL ,FOLIO_FISCAL ,NO_APROBACION ,SELLO_FE ,CADENA_ORIGINAL ,ANO_APROBACION ,TIPO_COMPROBANTE ,ESTATUS_FE ,MENSAJE_FE ,NO_CERTIFICADO ,TIMBRE_UUID ,FECHA_TIMBRADO ,SELLO_SAT ,CERTIFICADO_SAT ,REGIMENES ,COND_PAGO ,METODO_PAGO ,NUMCTA_PAGO ,LUGAR_EXPED ,TIPO_MONEDA ,CONCILIADA ,CVEPAGO_SAT) values " +
                                    "('" + FOLIO_INTERNO[i].InnerText + "', '" + SERIE_FISCAL[i].InnerText + "', '" + FOLIO_FISCAL[i].InnerText + "', '" + NO_APROBACION[i].InnerText + "', '" + SELLO_FE[i].InnerText + "', '" + CADENA_ORIGINAL[i].InnerText + "', " + ANO_APROBACION[i].InnerText + ", " + TIPO_COMPROBANTE[i].InnerText + ", " + ESTATUS_FE[i].InnerText + ", '" + MENSAJE_FE[i].InnerText + "', '" + NO_CERTIFICADO[i].InnerText + "', '" + TIMBRE_UUID[i].InnerText + "', '" + FECHA_TIMBRADO[i].InnerText + "', '" + SELLO_SAT[i].InnerText + "', '" + CERTIFICADO_SAT[i].InnerText + "', '" + REGIMENES[i].InnerText + "', '" + COND_PAGO[i].InnerText + "', '" + METODO_PAGO[i].InnerText + "', '" + NUMCTA_PAGO[i].InnerText + "', " + LUGAR_EXPED[i].InnerText + ", '" + TIPO_MONEDA[i].InnerText + "', " + CONCILIADA[i].InnerText + ", '" + CVEPAGO_SAT[i].InnerText + "')";
                            }
                            else
                            {
                                sentencia = "UPDATE tblFacturaElectronica SET " +
                                    " SERIE_FISCAL='" + SERIE_FISCAL[i].InnerText + "', FOLIO_FISCAL='" + FOLIO_FISCAL[i].InnerText + "', NO_APROBACION='" + NO_APROBACION[i].InnerText + "', SELLO_FE='" + SELLO_FE[i].InnerText + "', CADENA_ORIGINAL='" + CADENA_ORIGINAL[i].InnerText + "', ANO_APROBACION=" + ANO_APROBACION[i].InnerText + ", TIPO_COMPROBANTE=" + TIPO_COMPROBANTE[i].InnerText + ", ESTATUS_FE=" + ESTATUS_FE[i].InnerText + ", MENSAJE_FE='" + MENSAJE_FE[i].InnerText + "', NO_CERTIFICADO='" + NO_CERTIFICADO[i].InnerText + "', TIMBRE_UUID='" + TIMBRE_UUID[i].InnerText + "', FECHA_TIMBRADO='" + FECHA_TIMBRADO[i].InnerText + "', SELLO_SAT='" + SELLO_SAT[i].InnerText + "', CERTIFICADO_SAT='" + CERTIFICADO_SAT[i].InnerText + "', REGIMENES='" + REGIMENES[i].InnerText + "', COND_PAGO='" + COND_PAGO[i].InnerText + "', METODO_PAGO='" + METODO_PAGO[i].InnerText + "', NUMCTA_PAGO='" + NUMCTA_PAGO[i].InnerText + "', LUGAR_EXPED=" + LUGAR_EXPED[i].InnerText + ", TIPO_MONEDA='" + TIPO_MONEDA[i].InnerText + "', CONCILIADA=" + CONCILIADA[i].InnerText + ", CVEPAGO_SAT='" + CVEPAGO_SAT[i].InnerText + "' " +
                                    " WHERE FOLIO_INTERNO='" + FOLIO_INTERNO[i].InnerText + "';";

                            }
                            BD.FunicionEjecucion(sentencia);
                        }
                    }
                    #endregion



                }


                #endregion

                escribe(1, " ", nombre);
                //------Tipo 2
                escribe(1, "Tipo 2", nombre);
                #region Tipo 2
                XmlNodeList Tipo2 = xDoc.GetElementsByTagName("Tipo2");

                XmlNodeList listaT2;
                foreach (XmlElement nodo in Tipo2)
                {
                    #region Puntos
                    escribe(1, "Puntos", nombre);
                    listaT2 = nodo.GetElementsByTagName("Puntos");
                    foreach (XmlElement nodo1 in listaT2)
                    {
                        XmlNodeList Clave = nodo1.GetElementsByTagName("Clave");
                        XmlNodeList ValorNum = nodo1.GetElementsByTagName("ValorNum");
                        string sentencia = ""; decimal PTO_XCOM = 0, PTO_CONS = 0, PTO_DISP = 0;
                        DataTable dtDatosCliente = BD.datatableBD("SELECT * FROM tblpuntosxcliente WHERE COD_CLI= '" + Clave[0].InnerText + "'");
                        PTO_XCOM = Convert.ToDecimal(ValorNum[0].InnerText);
                        if (PTO_XCOM < 0)
                        {
                            PTO_CONS = PTO_XCOM;
                            PTO_XCOM = 0;
                        }

                        if (dtDatosCliente.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtDatosCliente.Rows)
                            {
                                PTO_XCOM = PTO_XCOM + Convert.ToDecimal(row["PTO_XCOM"].ToString());
                                PTO_CONS = PTO_CONS + Convert.ToDecimal(row["PTO_CONS"].ToString());
                            }
                            PTO_DISP = PTO_XCOM + PTO_CONS;

                            sentencia = "UPDATE tblpuntosxcliente SET PTO_XCOM = " + PTO_XCOM + ", PTO_CONS = " + PTO_CONS + ", PTO_DISP = " + PTO_DISP + " WHERE COD_CLI = '" + Clave[0].InnerText + "';";
                        }
                        else
                        {
                            PTO_DISP = PTO_XCOM + PTO_CONS;
                            sentencia = "INSERT INTO tblpuntosxcliente (COD_CLI, NIVEL, PTO_XCOM, PTO_CONS, PTO_DISP) VALUES (" +
                                "'" + Clave[0].InnerText + "', 1, " + PTO_XCOM + ", " + PTO_CONS + ", " + PTO_DISP + ");";
                        }
                        BD.FunicionEjecucion(sentencia);
                        
                    }
                    #endregion
                }
                #endregion
                escribe(1, " ", nombre);


                //------Tipo 3
                escribe(1, "Tipo 3", nombre);
                #region Tipo 3
                XmlNodeList listaT3;
                XmlNodeList Tipo3 = xDoc.GetElementsByTagName("Tipo3");
                foreach (XmlElement nodo in Tipo3)
                {
                    #region Cancelaciones
                    escribe(1, "Cancelaciones", nombre);
                    listaT3 = nodo.GetElementsByTagName("Cancelaciones");
                    foreach (XmlElement nodo1 in listaT3)
                    {
                        XmlNodeList Folio = nodo1.GetElementsByTagName("Folio");
                        XmlNodeList Tabla = nodo1.GetElementsByTagName("Tabla");
                        XmlNodeList Estatus = nodo1.GetElementsByTagName("Estatus");
                        string sentencia = "";
                        switch (Tabla[0].InnerText)
                        {
                            case "tblEncDevolucion":
                                if (BD.consulta("SELECT COUNT(*) FROM " + Tabla[0].InnerText + " WHERE FOL_DEV = '" + Folio[0].InnerText + "'") != "0")
                                {
                                    sentencia = "UPDATE " + Tabla[0].InnerText + " SET COD_STS = " + Estatus[0].InnerText + "  WHERE FOL_DEV = '" + Folio[0].InnerText + "'";
                                    BD.FunicionEjecucion(sentencia);
                                }
                                else
                                {
                                    escribe(1, "No existe el folio: " + Folio[0].InnerText + " ", nombre);
                                }
                                break;
                            case "tblEncCargosAbonos":
                                if (BD.consulta("SELECT COUNT(*) FROM " + Tabla[0].InnerText + " WHERE FOL_DOC = '" + Folio[0].InnerText + "'") != "0")
                                {
                                    sentencia = "UPDATE " + Tabla[0].InnerText + " SET COD_STS = " + Estatus[0].InnerText + "  WHERE FOL_DOC = '" + Folio[0].InnerText + "'";
                                    BD.FunicionEjecucion(sentencia);
                                    #region Saldo
                                    string COD_CLI = BD.consulta("SELECT SAL_CLI FROM " + Tabla[0].InnerText + " WHERE FOL_DOC = '" + Folio[0].InnerText + "'");
                                    string CON_GRL = BD.consulta("SELECT CON_GRL FROM " + Tabla[0].InnerText + " WHERE FOL_DOC = '" + Folio[0].InnerText + "'");
                                    string IMP_DOC = BD.consulta("SELECT IMP_DOC FROM " + Tabla[0].InnerText + " WHERE FOL_DOC = '" + Folio[0].InnerText + "'");
                                    string datototal = BD.consulta("SELECT SAL_CLI FROM tblCatClientes WHERE COD_CLI = '" + COD_CLI + "'");


                                    decimal SaldoTotal = Convert.ToDecimal(datototal);

                                    if (CON_GRL == "CCLI")
                                    {
                                        SaldoTotal = SaldoTotal + Convert.ToDecimal(IMP_DOC);
                                    }
                                    else
                                    {
                                        if (CON_GRL == "ACLI")
                                        {
                                            SaldoTotal = SaldoTotal - Convert.ToDecimal(IMP_DOC);
                                        }
                                    }

                                    BD.FunicionEjecucion("UPDATE tblCatClientes SET SAL_CLI = " + SaldoTotal + " WHERE COD_CLI = '" + COD_CLI + "'");
                                    #endregion
                                }
                                break;
                            case "tblRenCargosAbonos":
                                if (BD.consulta("SELECT COUNT(*) FROM " + Tabla[0].InnerText + " WHERE FOL_DOC = '" + Folio[0].InnerText + "'") != "0")
                                {
                                    sentencia = "UPDATE " + Tabla[0].InnerText + " SET COD_STS = " + Estatus[0].InnerText + "  WHERE FOL_DOC = '" + Folio[0].InnerText + "'";
                                    BD.FunicionEjecucion(sentencia);
                                }
                                break;
                            case "tblFacturaElectronica":
                                if (BD.consulta("SELECT COUNT(*) FROM " + Tabla[0].InnerText + " WHERE FOLIO_INTERNO = '" + Folio[0].InnerText + "'") != "0")
                                {
                                    sentencia = "UPDATE " + Tabla[0].InnerText + " SET ESTATUS_FE = " + Estatus[0].InnerText + "  WHERE FOLIO_INTERNO = '" + Folio[0].InnerText + "'";
                                    BD.FunicionEjecucion(sentencia);
                                }
                                break;
                            case "tblFacturasEnc":
                                if (BD.consulta("SELECT COUNT(*) FROM " + Tabla[0].InnerText + " WHERE FOLIO_FAC = '" + Folio[0].InnerText + "'") != "0")
                                {
                                    sentencia = "UPDATE " + Tabla[0].InnerText + " SET STS_DOC = " + Estatus[0].InnerText + "  WHERE FOLIO_FAC = '" + Folio[0].InnerText + "'";
                                    BD.FunicionEjecucion(sentencia);
                                }
                                break;
                            case "tblNotasPorFactura":
                                DataTable dtDatos = BD.datatableBD("SELECT FOLIO_NV FROM tblNotasPorFactura WHERE FOLIO_FACT = '" + Folio[0].InnerText + "';");
                                foreach (DataRow row in dtDatos.Rows)
                                {
                                    sentencia = "UPDATE tblGralVentas SET STS_DOC = 2 WHERE REF_DOC = '" + row["FOLIO_NV"].ToString() + "'; ";
                                    BD.FunicionEjecucion(sentencia);
                                }
                                sentencia = "UPDATE tblEncCargosAbonos SET FOL_FACT='' WHERE FOL_FACT='" + Folio[0].InnerText + "';";
                                BD.FunicionEjecucion(sentencia);
                                sentencia = "DELETE FROM tblNotasPorFactura WHERE FOLIO_FACT = '" + Folio[0].InnerText + "';";
                                BD.FunicionEjecucion(sentencia);
                                break;
                        }

                    }
                    #endregion
                }
                #endregion

             

                escribe(1, " ", nombre);
                

                status = "33";
                string sentenciaX = "UPDATE tblac_paquetes Set Envio_Recep = 33, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + NombrePaq + "'";

                BD.GuardaCambios(sentenciaX);

                escribe(1, status, nombre);
                //escribe(1, sentenciaX, nombre);
                //}
            }
            catch (System.OutOfMemoryException ex)
            {
                error = true; status = "29";
                mensajeHilo = "hilo: " + hilo + " " + ex.Message + " " + ex.InnerException + " " + ex.StackTrace;
            }
            catch (Exception err)
            {
                error = true; status = "29";
                mensajeHilo = "hilo: " + hilo + " " + err.Message + " " + err.InnerException + " " + err.StackTrace;
            }
            finally
            {

                escribe(1, "Fin de lectura - " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"), nombre);
                escribeArchivo(nombre);
                BD.conexionMySQL.Dispose();
                BD.conexionMSSQL.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

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





    }
}
