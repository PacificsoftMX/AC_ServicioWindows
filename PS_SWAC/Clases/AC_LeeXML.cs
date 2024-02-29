using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace PS_SWAC.Clases
{
    class AC_LeeXML : IDisposable
    {
        public string cnnxion, mensajeSentencia;
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
            string date;

            if (fechax != "")
            {
                if (!fechax.Contains("-"))
                {
                    date = fechax.Substring(6, 4) + fechax.Substring(3, 2) + fechax.Substring(0, 2);
                }
                else
                {
                    date = fechax;
                }
            }
            else { date = "18000101"; }

            return date;
        }

        private void escribe(int numero, string dato, string archivo)
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
            }
        }

        public void LeeXml(string archivoXML, string rutaPath, int hilo)
        {
            #region Base de datos
            string ruta1 = Application.StartupPath;
            string idEmpresa = "";
            funcionBD BD = new funcionBD();
            funcion funciones = new funcion();
            BD.conexionD = BD.ConexionDelfin();
            idEmpresa = funciones.LeerArchivoINI("VARIOS", "EMPRESA", ruta1);
            BD.conexionAC = BD.ConexionBD(idEmpresa, "CONX_AC");
            BD.conexionPV = BD.ConexionBD(idEmpresa, "CONEXION");
            #endregion
            //MessageBox.Show("clsFolios");
            //PS_BaseDatos.clsFormato objFormatoBD = new PS_BaseDatos.clsFormato();
            //PS_FuncionesVB.clsFolios objFolios = new PS_FuncionesVB.clsFolios();
            // MessageBox.Show("clsCostos");
            PS_FuncionesVB.clsCostos objCostos = new PS_FuncionesVB.clsCostos();
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

            StreamWriter writer = File.CreateText(nombre);
            writer.Close();


            XmlNodeList lista;
            int numerodedatos, numerodedatos1;

            #region variables
            String[,] DatosNotaDeVenta = null;//= new string[99999, 31];
            String[,] DatosNotaDeVenta1 = null;// = new string[99999, 31];
            String[,] DatosNotaDeVenta2 = null;// = new string[99999, 31];

            String[,] DatosFactura = null;// = new string[1, 22];
            String[,] DatosFactura1 = null;//= new string[99999, 23];
            String[,] DatosFactura2 = null;//= new string[99999, 2];
            String[,] DatosFactura3 = null;//= new string[99999, 4];

            String[,] DatosPedidos1 = null;//= new string[99999, 31];
            String[,] DatosPedidos2 = null;//= new string[99999, 15];

            String[,] DatosAuxiliarCaja = null;//= new string[99999, 33];

            String[,] DatosCartera = null;//= new string[99999, 23];
            String[,] DatosCartera1 = new string[99999, 10];

            String[,] datos = new string[99999, 12];
            String[,] datos1 = null;//= new string[99999, 23];

            String[] foliosN = new string[999999];
            String[,] FOCATI = new string[999999, 3];
            #endregion
            //try
            //{
            escribe(1, "hilo: " + hilo + " Inicio de lectura - " + DateTime.Now.ToString(), nombre);
            //-------------------------------------------------------------------------------------------------------------
            #region Nota_de_Venta
            XmlNodeList Nota_de_Venta = xDoc.GetElementsByTagName("Nota_de_Venta");
            numerodedatos = Nota_de_Venta.Count;
            string clienteNV1, usuarioNV1, mesaNV1;
            int caja1, empresaNV1, cfn;
            cfn = 0;
            escribe(1, "Datos Generales NV", nombre);
            escribe(2, " ", nombre);
            DatosNotaDeVenta = new string[numerodedatos, 31];
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Nota_de_Venta[z]).GetElementsByTagName("Datos_Generales_NV");

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

                    DatosNotaDeVenta[z, 0] = tipo_de_movimiento[i].InnerText;
                    DatosNotaDeVenta[z, 1] = folioNV[i].InnerText;
                    DatosNotaDeVenta[z, 2] = folio_general[i].InnerText;
                    DatosNotaDeVenta[z, 3] = TIP[i].InnerText;
                    DatosNotaDeVenta[z, 4] = clienteNV[i].InnerText;
                    DatosNotaDeVenta[z, 5] = fechaNV[i].InnerText;
                    DatosNotaDeVenta[z, 6] = fecharegistroNV[i].InnerText;
                    DatosNotaDeVenta[z, 7] = horaNV[i].InnerText;
                    DatosNotaDeVenta[z, 8] = caja[i].InnerText;
                    DatosNotaDeVenta[z, 9] = turno[i].InnerText;
                    DatosNotaDeVenta[z, 10] = subtotalNV[i].InnerText;
                    DatosNotaDeVenta[z, 11] = impuestoNV[i].InnerText;
                    DatosNotaDeVenta[z, 12] = importe_exentoNV[i].InnerText;
                    DatosNotaDeVenta[z, 13] = totalNV[i].InnerText;
                    DatosNotaDeVenta[z, 14] = total_pagadoNV[i].InnerText;
                    DatosNotaDeVenta[z, 15] = saldoNV[i].InnerText;
                    DatosNotaDeVenta[z, 16] = impuesto_integradoNV[i].InnerText;
                    DatosNotaDeVenta[z, 17] = estatusNV[i].InnerText;
                    DatosNotaDeVenta[z, 18] = folio_liqNV[i].InnerText;
                    DatosNotaDeVenta[z, 19] = notasNV[i].InnerText;
                    DatosNotaDeVenta[z, 20] = usuarioNV[i].InnerText;
                    DatosNotaDeVenta[z, 21] = descuentoNV[i].InnerText;
                    DatosNotaDeVenta[z, 22] = cargosNV[i].InnerText;
                    DatosNotaDeVenta[z, 23] = plazoNV[i].InnerText;
                    DatosNotaDeVenta[z, 24] = vencimientoNV[i].InnerText;
                    DatosNotaDeVenta[z, 25] = sucursalNV[i].InnerText;
                    DatosNotaDeVenta[z, 26] = contabilizadaNV[i].InnerText;
                    DatosNotaDeVenta[z, 27] = creditoNV[i].InnerText;
                    DatosNotaDeVenta[z, 28] = importe_creditoNV[i].InnerText;
                    DatosNotaDeVenta[z, 29] = mesaNV[i].InnerText;
                    DatosNotaDeVenta[z, 30] = empresaNV[i].InnerText;



                    if (BD.consulta("SELECT COUNT(*) FROM tblCatClientes WHERE COD_CLI ='" + DatosNotaDeVenta[z, 4] + "'") == "0")
                    { clienteNV1 = "PUBLIC"; escribe(3, "No existe el cliente " + DatosNotaDeVenta[z, 4] + "para la NV " + DatosNotaDeVenta[z, 1], nombre); }
                    else { clienteNV1 = DatosNotaDeVenta[z, 4]; }

                    if (BD.consulta("SELECT COUNT(*) FROM tblCajas WHERE COD_CAJ =" + DatosNotaDeVenta[z, 8] + "") == "0")
                    { caja1 = 1; escribe(3, "No existe la caja " + DatosNotaDeVenta[z, 8], nombre); }
                    else { caja1 = Convert.ToInt32(DatosNotaDeVenta[z, 8]); }

                    if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosNotaDeVenta[z, 20] + "'") == "0")
                    { usuarioNV1 = "DEPURADO"; escribe(3, "No existe el usuario " + DatosNotaDeVenta[z, 20], nombre); }
                    else { usuarioNV1 = DatosNotaDeVenta[z, 20]; }

                    if (DatosNotaDeVenta[z, 29] != "")
                    {
                        if (BD.consulta("SELECT COUNT(*) FROM tblMesas WHERE COD_MESA ='" + DatosNotaDeVenta[z, 29] + "'") == "0")
                        { mesaNV1 = "1"; escribe(3, "No existe la mesa " + DatosNotaDeVenta[z, 29], nombre); }
                        else { mesaNV1 = DatosNotaDeVenta[z, 29]; }
                    }
                    else
                    {
                        mesaNV1 = DatosNotaDeVenta[z, 29];
                    }

                    if (BD.consulta("SELECT COUNT(*) FROM tblEmpresa WHERE COD_EMPRESA =" + Convert.ToInt32(DatosNotaDeVenta[z, 30]) + "") == "0")
                    { empresaNV1 = 1; escribe(3, "No existe la empresa " + DatosNotaDeVenta[z, 30], nombre); }
                    else { empresaNV1 = Convert.ToInt32(DatosNotaDeVenta[z, 30]); }




                    if (BD.consulta("SELECT COUNT(*) FROM tblGralVentas WHERE REF_DOC ='" + DatosNotaDeVenta[z, 1] + "'") == "0")
                    {
                        if (BD.consulta("SELECT COUNT(*) FROM tblGralVentas WHERE FOL_GRL ='" + DatosNotaDeVenta[z, 2] + "'") == "0")
                        {
                            //Datos que no vienen en el XML:
                            //CON_GRL = NVEN
                            //ENVIADO = 0
                            string fecha1, fecha2, fecha3;
                            fecha1 = DatosNotaDeVenta[z, 5].Substring(6, 4) + DatosNotaDeVenta[z, 5].Substring(3, 2) + DatosNotaDeVenta[z, 5].Substring(0, 2);
                            fecha2 = DatosNotaDeVenta[z, 6].Substring(6, 4) + DatosNotaDeVenta[z, 6].Substring(3, 2) + DatosNotaDeVenta[z, 6].Substring(0, 2);
                            fecha3 = DatosNotaDeVenta[z, 24].Substring(6, 4) + DatosNotaDeVenta[z, 24].Substring(3, 2) + DatosNotaDeVenta[z, 24].Substring(0, 2);

                            BD.GuardaCambios("INSERT INTO tblgralventas(REF_DOC, FOL_GRL, COD_CLI, FEC_DOC, FEC_REG, HORA_REG, CAJA_DOC, CAJA_TUR, SUB_DOC, IVA_DOC, IMPTO_IMPTOT, TOT_DOC, TOT_PAG, TOTAL_TIP, SAL_DOC, IMPTO_INT, STS_DOC, FOL_LIQ, NOTA, COD_USU, DES_CLI, CAR1_VEN, PLA_PAG, FEC_VENC, COD_SUCU, CONTAB, CREDITO, IMPORTE_CRED, COD_MESA, COD_EMPRESA, ENVIADO) VALUES ('" + DatosNotaDeVenta[z, 1] + "', '" + DatosNotaDeVenta[z, 2] + "', '" + clienteNV1 + "', '" + fecha1 + "', '" + fecha2 + "', '" + DatosNotaDeVenta[z, 7] + "', " + Convert.ToInt64(caja1) + ", " + Convert.ToInt16(DatosNotaDeVenta[z, 9]) + ", " + verificaLongitud(DatosNotaDeVenta[z, 10]) + ", " + verificaLongitud(DatosNotaDeVenta[z, 11]) + ", " + verificaLongitud(DatosNotaDeVenta[z, 12]) + ", " + verificaLongitud(DatosNotaDeVenta[z, 13]) + ", " + verificaLongitud(DatosNotaDeVenta[z, 14]) + ", " + Convert.ToDecimal(DatosNotaDeVenta[z, 3]) + ", " + verificaLongitud(DatosNotaDeVenta[z, 15]) + ", " + Convert.ToDecimal(DatosNotaDeVenta[z, 16]) + ", " + Convert.ToInt16(DatosNotaDeVenta[z, 17]) + ", '" + DatosNotaDeVenta[z, 18] + "',  '" + DatosNotaDeVenta[z, 19] + "', '" + usuarioNV1 + "', " + Convert.ToDecimal(DatosNotaDeVenta[z, 21]) + ", " + Convert.ToDecimal(DatosNotaDeVenta[z, 22]) + ", " + Convert.ToInt64(DatosNotaDeVenta[z, 23]) + ", '" + fecha3 + "', '" + DatosNotaDeVenta[z, 25] + "', " + Convert.ToInt16(DatosNotaDeVenta[z, 26]) + ", " + Convert.ToInt16(DatosNotaDeVenta[z, 27]) + ", " + verificaLongitud(DatosNotaDeVenta[z, 28]) + ", '" + mesaNV1 + "', " + empresaNV1 + ", 0)");

                            foliosN[cfn] = DatosNotaDeVenta[z, 1];
                            cfn = cfn + 1;
                        }
                        else
                        {
                            escribe(3, "Ya existe la Nota de Venta FOL_GRL " + DatosNotaDeVenta[z, 2], nombre);
                        }

                    }
                    else
                    {
                        escribe(3, "Ya existe la Nota de Venta REF_DOC " + DatosNotaDeVenta[z, 1], nombre);
                    }
                }
            }

            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Partidas_NV", nombre);
            DatosNotaDeVenta1 = new string[numerodedatos, 31];
            for (int z = 0; z < numerodedatos; z++)
            {
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

                    DatosNotaDeVenta1[z, 0] = folio[i].InnerText;
                    DatosNotaDeVenta1[z, 1] = articulo[i].InnerText;
                    DatosNotaDeVenta1[z, 2] = vendedor[i].InnerText;
                    DatosNotaDeVenta1[z, 3] = almacen[i].InnerText;
                    DatosNotaDeVenta1[z, 4] = cantidad[i].InnerText;
                    DatosNotaDeVenta1[z, 5] = unidad[i].InnerText;
                    DatosNotaDeVenta1[z, 6] = equivalencia[i].InnerText;
                    DatosNotaDeVenta1[z, 7] = precio_catalogo[i].InnerText;
                    DatosNotaDeVenta1[z, 8] = precio_venta[i].InnerText;
                    DatosNotaDeVenta1[z, 9] = moneda[i].InnerText;
                    DatosNotaDeVenta1[z, 10] = tipo_de_cambio[i].InnerText;
                    DatosNotaDeVenta1[z, 11] = porcentaje_descto[i].InnerText;
                    DatosNotaDeVenta1[z, 12] = descto_adicional[i].InnerText;
                    DatosNotaDeVenta1[z, 13] = codigo_impto1[i].InnerText;
                    DatosNotaDeVenta1[z, 14] = codigo_impto2[i].InnerText;
                    DatosNotaDeVenta1[z, 15] = importe_impto1[i].InnerText;
                    DatosNotaDeVenta1[z, 16] = importe_impto2[i].InnerText;
                    DatosNotaDeVenta1[z, 17] = porcentaje_impto1[i].InnerText;
                    DatosNotaDeVenta1[z, 18] = porcentaje_impto2[i].InnerText;
                    DatosNotaDeVenta1[z, 19] = importe_exento[i].InnerText;
                    DatosNotaDeVenta1[z, 20] = devueltos[i].InnerText;
                    DatosNotaDeVenta1[z, 21] = costo_de_venta[i].InnerText;
                    DatosNotaDeVenta1[z, 22] = costo_peps[i].InnerText;
                    DatosNotaDeVenta1[z, 23] = porcentaje_comision[i].InnerText;
                    DatosNotaDeVenta1[z, 24] = importe_sindescuento[i].InnerText;
                    DatosNotaDeVenta1[z, 25] = descuento_general[i].InnerText;
                    //20150320 validacion que se puso, para que procese paquetes antes de la actualizacion
                    try
                    {
                        DatosNotaDeVenta1[z, 26] = CLAVE_OFR[i].InnerText;
                    }
                    catch
                    {
                        DatosNotaDeVenta1[z, 26] = "";
                    }


                    for (int r = 0; foliosN.Length > r; r++)
                    {
                        if (foliosN[r] != "" && foliosN[r] != null)
                        {
                            if (foliosN[r] == DatosNotaDeVenta1[z, 0])
                            {
                                string articuloN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblCatArticulos WHERE COD1_ART ='" + DatosNotaDeVenta1[z, 1] + "'") == "0")
                                {
                                    articuloN = "DEPURADO";
                                    escribe(3, "No existe el artículo" + DatosNotaDeVenta1[z, 1], nombre);
                                    //“No existe el artículo” + <articulo> DatosNotaDeVenta1[z, 1]
                                }
                                else
                                {
                                    articuloN = DatosNotaDeVenta1[z, 1];
                                }

                                string vendedorN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblVendedores WHERE COD_VEN ='" + DatosNotaDeVenta1[z, 2] + "'") == "0")
                                {
                                    vendedorN = "PISO"; //“No existe el vendedor” + <vendedor> DatosNotaDeVenta1[z, 2]
                                    escribe(3, "No existe el vendedo" + DatosNotaDeVenta1[z, 2], nombre);
                                }
                                else
                                {
                                    vendedorN = DatosNotaDeVenta1[z, 2];
                                }

                                string almacenN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + DatosNotaDeVenta1[z, 3] + "'") == "0")
                                {
                                    almacenN = funciones.LeerArchivoINI("FAST FOOD", "MERCANCIA", Application.StartupPath);
                                    escribe(3, "No existe el almacen" + DatosNotaDeVenta1[z, 3], nombre);//“No existe el almacen” + <almacen>. DatosNotaDeVenta1[z, 3]
                                }
                                else
                                {
                                    almacenN = DatosNotaDeVenta1[z, 3];
                                }

                                decimal unidadN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloN + "' AND COD_UND ='" + DatosNotaDeVenta1[z, 5] + "'") == "0")
                                {
                                    unidadN = 1;
                                    escribe(3, "No coincide la equivalencia del articulo " + DatosNotaDeVenta1[z, 1] + " para la unidad " + DatosNotaDeVenta1[z, 5], nombre);//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[z,6]
                                }
                                else
                                {
                                    unidadN = Convert.ToDecimal(DatosNotaDeVenta1[z, 6]);
                                }

                                Int16 monedaN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosNotaDeVenta1[z, 9]) + "") == "0")
                                {
                                    monedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[z, 9]
                                    escribe(3, "No existe la moneda" + DatosNotaDeVenta1[z, 9] + " de la nota de venta " + DatosNotaDeVenta1[z, 0], nombre);//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[z,6]
                                }
                                else
                                {
                                    monedaN = Convert.ToInt16(DatosNotaDeVenta1[z, 9]);
                                }

                                int codigo_impto1N;
                                if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosNotaDeVenta1[z, 13]) + "") == "0")
                                {
                                    codigo_impto1N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[z, 13]
                                    escribe(3, "No existe el impuesto" + DatosNotaDeVenta1[z, 13], nombre);//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[z,6]

                                }
                                else
                                {
                                    codigo_impto1N = Convert.ToInt32(DatosNotaDeVenta1[z, 13]);
                                }

                                int codigo_impto2N;
                                if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosNotaDeVenta1[z, 14]) + "") == "0")
                                {
                                    codigo_impto2N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[z, 14]
                                    escribe(3, "No existe el impuesto" + DatosNotaDeVenta1[z, 14], nombre);
                                }
                                else
                                {
                                    codigo_impto2N = Convert.ToInt32(DatosNotaDeVenta1[z, 14]);

                                }
                                BD.GuardaCambios("INSERT INTO tblRenVentas(REF_DOC, COD1_ART, COD_VEN, COD_ALM, CAN_ART, COD_UND, EQV_UND, PCIO_UNI, PCIO_VEN, COD_MON, TIP_CAM, POR_DES, DECTO_ADI, COD1_IMP, COD2_IMP, IMP1_REG, IMP2_REG, IMP1_ART, IMP2_ART, IMPTO_IMP, CAN_DEV, COS_VEN, COS_PEPS, POR_COM, FOL_GRL, IMP_SINDESC, DCTO_GRAL, NUM_LOT, FEC_CAD, CLAVE_OFR)VALUES('" + DatosNotaDeVenta1[z, 0] + "', '" + articuloN + "', '" + vendedorN + "', '" + almacenN + "', " + Convert.ToDecimal(DatosNotaDeVenta1[z, 4]) + ", '" + DatosNotaDeVenta1[z, 5] + "', " + Convert.ToDecimal(unidadN) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[z, 7]) + ", " + verificaLongitud(DatosNotaDeVenta1[z, 8]) + ", " + monedaN + ", " + Convert.ToDouble(DatosNotaDeVenta1[z, 10]) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[z, 11]) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[z, 12]) + ", " + Convert.ToInt16(codigo_impto1N) + ", " + Convert.ToInt16(codigo_impto2N) + ", " + Convert.ToDecimal(DatosNotaDeVenta1[z, 15]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[z, 16]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[z, 17]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[z, 18]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[z, 19]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[z, 20]) + ",  " + verificaLongitud(DatosNotaDeVenta1[z, 21]) + ",  " + verificaLongitud(DatosNotaDeVenta1[z, 22]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[z, 23]) + ", '" + DatosNotaDeVenta1[z, 0] + "',  " + verificaLongitud(DatosNotaDeVenta1[z, 24]) + ",  " + Convert.ToDecimal(DatosNotaDeVenta1[z, 25]) + ", '', '1800-01-01',  '" + DatosNotaDeVenta1[z, 26] + "')");//Corregido 15-abr-2013.. Tenía comilla al final y no la tenía en el z, 0
                            }
                        }
                    }
                }
            }


            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Pagos_NV", nombre);
            DatosNotaDeVenta2 = new string[numerodedatos, 31];
            for (int z = 0; z < numerodedatos; z++)
            {
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



                    DatosNotaDeVenta2[z, 0] = folio[i].InnerText;
                    DatosNotaDeVenta2[z, 1] = folio_general[i].InnerText;
                    DatosNotaDeVenta2[z, 2] = referencia_general[i].InnerText;
                    DatosNotaDeVenta2[z, 3] = referencia_adicional[i].InnerText;
                    DatosNotaDeVenta2[z, 4] = concepto[i].InnerText;
                    DatosNotaDeVenta2[z, 5] = concepto_de_caja[i].InnerText;
                    DatosNotaDeVenta2[z, 6] = concepto_general[i].InnerText;
                    DatosNotaDeVenta2[z, 7] = caja[i].InnerText;
                    DatosNotaDeVenta2[z, 8] = turno[i].InnerText;
                    DatosNotaDeVenta2[z, 9] = usuario[i].InnerText;
                    DatosNotaDeVenta2[z, 10] = autoriza[i].InnerText;
                    DatosNotaDeVenta2[z, 11] = hora[i].InnerText;
                    DatosNotaDeVenta2[z, 12] = código_forma[i].InnerText;
                    DatosNotaDeVenta2[z, 13] = importe_pago[i].InnerText;
                    DatosNotaDeVenta2[z, 14] = moneda_pago[i].InnerText;
                    DatosNotaDeVenta2[z, 15] = tipo_de_cambio[i].InnerText;
                    DatosNotaDeVenta2[z, 16] = importe_pago_MN[i].InnerText;
                    DatosNotaDeVenta2[z, 17] = porcentaje_cargo[i].InnerText;
                    DatosNotaDeVenta2[z, 18] = importe_cargo[i].InnerText;
                    DatosNotaDeVenta2[z, 19] = referencia_pago[i].InnerText;
                    DatosNotaDeVenta2[z, 20] = saldo[i].InnerText;
                    DatosNotaDeVenta2[z, 21] = moneda_cambio[i].InnerText;
                    DatosNotaDeVenta2[z, 22] = importe_cambio[i].InnerText;
                    DatosNotaDeVenta2[z, 23] = tc_cambio[i].InnerText;
                    DatosNotaDeVenta2[z, 24] = numero_de_movimiento[i].InnerText;
                    DatosNotaDeVenta2[z, 25] = corte_virtual[i].InnerText;
                    DatosNotaDeVenta2[z, 26] = corte_parcial[i].InnerText;
                    DatosNotaDeVenta2[z, 27] = corte_final[i].InnerText;
                    DatosNotaDeVenta2[z, 28] = contabilizado[i].InnerText;
                    DatosNotaDeVenta2[z, 29] = sucursal[i].InnerText;
                    DatosNotaDeVenta2[z, 30] = cuenta_pago[i].InnerText;

                    for (int r = 0; foliosN.Length > r; r++)
                    {
                        if (foliosN[r] != "" && foliosN[r] != null)
                        {
                            if (foliosN[r] == DatosNotaDeVenta1[z, 0])
                            {
                                Int16 cajaNV;
                                if (BD.consulta("SELECT COUNT(*) FROM tblCajas WHERE COD_CAJ =" + Convert.ToInt32(DatosNotaDeVenta2[z, 7]) + "") == "0")
                                {
                                    cajaNV = 1;
                                }
                                else
                                {
                                    cajaNV = Convert.ToInt16(DatosNotaDeVenta2[z, 7]);
                                }

                                string codigoformaNV;
                                if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosNotaDeVenta2[z, 9] + "'") == "0")
                                {
                                    codigoformaNV = "DEPURADO"; //“No existe la forma de pago” + <código forma>.
                                }
                                else
                                {
                                    codigoformaNV = DatosNotaDeVenta2[z, 9];
                                }


                                string AUTusuarioNV123;
                                if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosNotaDeVenta2[z, 10] + "'") == "0")
                                {
                                    AUTusuarioNV123 = "DEPURADO";//“No existe el usuario” + <autoriza>  DatosNotaDeVenta2[z, 22], 

                                }
                                else
                                {
                                    AUTusuarioNV123 = DatosNotaDeVenta2[z, 10];
                                }

                                int codigoforma123;
                                if (BD.consulta("SELECT COUNT(*) FROM tblFormasPago WHERE COD_FRP =" + Convert.ToInt32(DatosNotaDeVenta2[z, 12]) + "") == "0")
                                {
                                    codigoforma123 = 1;// “No existe la forma de pago” + <código forma>, 
                                    escribe(3, "No existe la forma de pago" + DatosNotaDeVenta2[z, 12], nombre);
                                }
                                else
                                {
                                    codigoforma123 = Convert.ToInt32(DatosNotaDeVenta2[z, 12]);
                                }


                                Int16 monedaNV11;
                                if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosNotaDeVenta2[z, 14]) + "") == "0")
                                {
                                    monedaNV11 = 1; //No existe la moneda de pago” + <moneda>.
                                    escribe(3, "No existe la moneda de pago" + DatosNotaDeVenta2[z, 14], nombre);
                                }
                                else
                                {
                                    monedaNV11 = Convert.ToInt16(DatosNotaDeVenta2[z, 14]);
                                }

                                Int16 monedacambio123;
                                if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosNotaDeVenta2[z, 21]) + "") == "0")
                                {
                                    monedacambio123 = 1; //No existe la moneda de pago” + <moneda>.
                                    escribe(3, "No existe el cambio de moneda" + DatosNotaDeVenta2[z, 21], nombre);
                                }
                                else
                                {
                                    monedacambio123 = Convert.ToInt16(DatosNotaDeVenta2[z, 21]);
                                }
                                BD.GuardaCambios("INSERT INTO tblAuxCaja(REF_DOC, FOL_GRL, REF_GRL, REF_ADI, CON_CEP, COD_CON, CON_GRL, COD_CAJ, TUR_CAJ, COD_USU, USU_AUT, HORA_DOC, COD_FRP, IMP_EXT, COD_MON, TIP_CAM, IMP_MBA, POR_CAR, IMP_CAR, REF_PAG, SAL_DOC, MON_CAMBIO, IMPE_CAMBIO, TC_CAMBIO, FOL_VIR, FOL_PAR, FOL_FIN, CONTAB, FEC_DOC, COD_CLI, ENVIADO, COD_SUCU, CTA_PAGO, FOL_COR, NOTAS) VALUES ('" + DatosNotaDeVenta2[z, 0] + "', '" + DatosNotaDeVenta2[z, 1] + "', '" + DatosNotaDeVenta2[z, 2] + "', '" + DatosNotaDeVenta2[z, 3] + "', '" + DatosNotaDeVenta2[z, 4] + "', '" + DatosNotaDeVenta2[z, 5] + "', '" + DatosNotaDeVenta2[z, 6] + "', " + cajaNV + ", " + Convert.ToInt16(DatosNotaDeVenta2[z, 8]) + ", '" + codigoformaNV + "', '" + AUTusuarioNV123 + "', '" + DatosNotaDeVenta2[z, 11] + "', " + Convert.ToInt16(codigoforma123) + ", " + verificaLongitud(DatosNotaDeVenta2[z, 13]) + ", " + monedaNV11 + ", " + Convert.ToDecimal(DatosNotaDeVenta2[z, 15]) + ", " + verificaLongitud(DatosNotaDeVenta2[z, 16]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[z, 17]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[z, 18]) + ", '" + DatosNotaDeVenta2[z, 19] + "', " + verificaLongitud(DatosNotaDeVenta2[z, 20]) + ", " + monedacambio123 + ", " + verificaLongitud(DatosNotaDeVenta2[z, 22]) + ", " + Convert.ToDecimal(DatosNotaDeVenta2[z, 23]) + ", '" + DatosNotaDeVenta2[z, 25] + "', '" + DatosNotaDeVenta2[z, 26] + "', '" + DatosNotaDeVenta2[z, 27] + "', " + Convert.ToInt16(DatosNotaDeVenta2[z, 28]) + ", '" + CambioDeFecha(BD.consulta("SELECT FEC_DOC FROM tblGralVentas WHERE REF_DOC ='" + DatosNotaDeVenta2[z, 0] + "'")) + "', '" + BD.consulta("SELECT COD_CLI FROM tblgralventas WHERE REF_DOC ='" + DatosNotaDeVenta2[z, 0] + "'") + "', 0,'" + DatosNotaDeVenta2[z, 29] + "', '" + DatosNotaDeVenta2[z, 30] + "', '', '')");
                                //Datos que no vienen en el XML:
                                //FEC_DOC =  tblGralVentas.FEC_DOC
                                //COD_CLI =  tblGralVentas.COD_CLI
                                //ENVIADO = 0
                            }
                        }
                    }
                }
            }

            #endregion
            //-------------------------------------------------------------------------------------------------------------
            #region Facturas
            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Facturas", nombre);
            //MessageBox.Show("Facturas");
            XmlNodeList Facturas = xDoc.GetElementsByTagName("Facturas");
            numerodedatos = Facturas.Count;
            DatosFactura = new string[numerodedatos, 22];
            cfn = 0;
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Datos_Generales_Factura");

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

                    DatosFactura[z, 0] = tipo_de_movimiento[i].InnerText;
                    DatosFactura[z, 1] = foliofact[i].InnerText;
                    DatosFactura[z, 2] = folio_general[i].InnerText;
                    DatosFactura[z, 3] = concepto[i].InnerText;
                    DatosFactura[z, 4] = clientefact[i].InnerText;
                    DatosFactura[z, 5] = fechafact[i].InnerText;
                    DatosFactura[z, 6] = subtotalfact[i].InnerText;
                    DatosFactura[z, 7] = impuestofact[i].InnerText;
                    DatosFactura[z, 8] = importe_exentofact[i].InnerText;
                    DatosFactura[z, 9] = totalfact[i].InnerText;
                    DatosFactura[z, 10] = impuesto_integrado_fact[i].InnerText;
                    DatosFactura[z, 11] = tip_fact[i].InnerText;
                    DatosFactura[z, 12] = estatusfact[i].InnerText;
                    DatosFactura[z, 13] = notasfact[i].InnerText;
                    DatosFactura[z, 14] = usuariofact[i].InnerText;
                    DatosFactura[z, 15] = sucursalfact[i].InnerText;
                    DatosFactura[z, 16] = descuentofact[i].InnerText;
                    DatosFactura[z, 17] = cargosfact[i].InnerText;
                    DatosFactura[z, 18] = vencimientofact[i].InnerText;
                    DatosFactura[z, 19] = creditofact[i].InnerText;
                    DatosFactura[z, 20] = importe_creditofact[i].InnerText;
                    DatosFactura[z, 21] = empresafact[i].InnerText;


                    string conceptoDF;
                    if (BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE COD_CON ='" + DatosFactura[z, 3] + "'") == "0")
                    {
                        conceptoDF = BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE TIP_MOV ='FACT'");
                        escribe(3, "No existe la serie de factura" + DatosFactura[z, 3] + " se asignó" + conceptoDF, nombre);
                    }
                    else
                    {
                        conceptoDF = DatosFactura[z, 3];
                    }


                    string clienteNV123;
                    if (BD.consulta("SELECT COUNT(*) FROM tblCatClientes WHERE COD_CLI ='" + DatosFactura[z, 4] + "'") == "0")
                    {
                        clienteNV123 = "PUBLIC";//“No existe el cliente” + <clienteNV> + “para la Factura” + <foliofact> DatosFactura[z, 3], 
                        escribe(3, "No existe el cliente" + DatosFactura[z, 4] + " para la Factura " + DatosFactura[z, 1], nombre);

                    }
                    else
                    {
                        clienteNV123 = DatosFactura[z, 4];
                    }

                    string usuarioNV123;
                    if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosFactura[z, 14] + "'") == "0")
                    {
                        usuarioNV123 = "DEPURADO";//“No existe el usuario” + <usuarioNV>  DatosFactura[z, 3], 
                        escribe(3, "No existe el usuario" + DatosFactura[z, 14], nombre);

                    }
                    else
                    {
                        usuarioNV123 = DatosFactura[z, 14];
                    }

                    Int16 empresaNV1234;
                    if (BD.consulta("SELECT COUNT(*) FROM tblEmpresa WHERE COD_EMPRESA =" + DatosFactura[z, 21]) == "0")
                    {
                        empresaNV1234 = 1;//“No existe la empresa” + <empresaNV>  DatosFactura[z, 21], 
                        escribe(3, "No existe la empresa " + DatosFactura[z, 21], nombre);

                    }
                    else
                    {
                        empresaNV1234 = Convert.ToInt16(DatosFactura[z, 21]);
                    }


                    if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOLIO_FAC ='" + DatosFactura[z, 1] + "'") != "0")
                    {
                        //““Ya existe la Factura” + FOLIO_FAC.> DatosFactura[z, 1]
                        escribe(3, "Ya existe la Factura con folio " + DatosFactura[z, 1], nombre);
                    }
                    else
                    {
                        if (BD.consulta("SELECT COUNT(*) FROM tblfacturasenc WHERE FOL_GRL ='" + DatosFactura[z, 2] + "'") != "0")
                        {
                            //““Ya existe la Factura” + FOLIO_FAC.> DatosFactura[z, 2]
                            escribe(3, "Ya existe la factura con folio general" + DatosFactura[z, 2], nombre);

                        }
                        else
                        {
                            BD.GuardaCambios("INSERT INTO tblfacturasenc(FOLIO_FAC, FOL_GRL, COD_CON, COD_CLI, FEC_FAC, SUB_DOC, IVA_DOC, IMPTO_IMPTOT, TOT_DOC, IMPTO_INT, TOTAL_TIP, STS_DOC, NOTA, COD_USU, DES_CLI, CAR1_VEN, FEC_VENC, COD_SUCU, CREDITO, IMPORTE_CRED, COD_EMPRESA, ENVIADO, HORA_FAC, FOLIO_DIG) VALUES ('" + DatosFactura[z, 1] + "', '" + DatosFactura[z, 2] + "', '" + conceptoDF + "', '" + clienteNV123 + "', '" + CambioDeFecha(DatosFactura[z, 5]) + "', " + verificaLongitud(DatosFactura[z, 6]) + ", " + verificaLongitud(DatosFactura[z, 7]) + ", " + Convert.ToDecimal(DatosFactura[z, 8]) + ", " + verificaLongitud(DatosFactura[z, 9]) + ", " + Convert.ToDecimal(DatosFactura[z, 10]) + ", " + Convert.ToDecimal(DatosFactura[z, 11]) + ", " + Convert.ToInt16(DatosFactura[z, 12]) + ", '" + DatosFactura[z, 13] + "', '" + usuarioNV123 + "', " + Convert.ToDecimal(DatosFactura[z, 15]) + ", " + verificaLongitud(DatosFactura[z, 16]) + ", '" + verificaLongitud(DatosFactura[z, 17]) + "', '" + DatosFactura[z, 18] + "', " + Convert.ToInt16(DatosFactura[z, 19]) + ", " + verificaLongitud(DatosFactura[z, 20]) + ", " + empresaNV1234 + ", 0, '00:00:00', 'S/N')");
                            //Datos que no vienen en el XML:
                            // ENVIADO = 0
                            foliosN[cfn] = DatosFactura[z, 1];
                            cfn = cfn + 1;
                        }
                    }
                }
            }

            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Partidas_Factura", nombre);
            DatosFactura1 = new string[numerodedatos, 23];
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Partidas_Factura");

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

                    DatosFactura1[z, 0] = folio[i].InnerText;
                    DatosFactura1[z, 1] = articulo[i].InnerText;
                    DatosFactura1[z, 2] = cantidad[i].InnerText;
                    DatosFactura1[z, 3] = unidad[i].InnerText;
                    DatosFactura1[z, 4] = equivalencia[i].InnerText;
                    DatosFactura1[z, 5] = precio_venta[i].InnerText;
                    DatosFactura1[z, 6] = moneda[i].InnerText;
                    DatosFactura1[z, 7] = tipo_de_cambio[i].InnerText;
                    DatosFactura1[z, 8] = porcentaje_descto[i].InnerText;
                    DatosFactura1[z, 9] = descto_adicional[i].InnerText;
                    DatosFactura1[z, 10] = codigo_impto1[i].InnerText;
                    DatosFactura1[z, 11] = codigo_impto2[i].InnerText;
                    DatosFactura1[z, 12] = porcentaje_impto1[i].InnerText;
                    DatosFactura1[z, 13] = porcentaje_impto2[i].InnerText;
                    DatosFactura1[z, 14] = importe_impto1[i].InnerText;
                    DatosFactura1[z, 15] = importe_impto2[i].InnerText;
                    DatosFactura1[z, 16] = importe_exento[i].InnerText;
                    DatosFactura1[z, 17] = numero_movimiento[i].InnerText;
                    DatosFactura1[z, 18] = importe_sindescuento[i].InnerText;
                    DatosFactura1[z, 19] = descuento_general[i].InnerText;
                    DatosFactura1[z, 20] = precio_uni[i].InnerText;
                    DatosFactura1[z, 21] = fecha_cad[i].InnerText;
                    DatosFactura1[z, 22] = numero_lot[i].InnerText;

                    for (int r = 0; foliosN.Length > r; r++)
                    {
                        if (foliosN[r] != "" && foliosN[r] != null)
                        {
                            if (foliosN[r] == DatosFactura1[z, 0])
                            {
                                string articuloN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblCatArticulos WHERE COD1_ART ='" + DatosFactura1[z, 1] + "'") == "0")
                                {
                                    articuloN = "DEPURADO"; //No existe el artículo” + <articulo> DatosFactura1[z, 1]
                                    escribe(3, "No existe el artículo" + DatosFactura1[z, 1], nombre);
                                }
                                else
                                {
                                    articuloN = DatosFactura1[z, 1];
                                }

                                string unidadN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloN + "' AND COD_UND ='" + DatosFactura1[z, 3] + "'") == "0")
                                {
                                    unidadN = "1";//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosNotaDeVenta1[z,3]
                                    escribe(3, "No coincide la equivalencia del articulo " + DatosFactura1[z, 1] + " para la unidad " + DatosFactura1[z, 3], nombre);
                                }
                                else
                                {
                                    unidadN = DatosFactura1[z, 3];
                                }

                                Int16 monedaN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosFactura1[z, 6]) + "") == "0")
                                {
                                    monedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[z, 6]
                                    escribe(3, "No existe la moneda" + DatosFactura1[z, 6], nombre);
                                }
                                else
                                {
                                    monedaN = Convert.ToInt16(DatosFactura1[z, 6]);
                                }

                                int codigo_impto1N;
                                if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosFactura1[z, 10]) + "") == "0")
                                {
                                    codigo_impto1N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[z, 10]
                                    escribe(3, "No existe el impuesto " + DatosFactura1[z, 10], nombre);

                                }
                                else
                                {
                                    codigo_impto1N = Convert.ToInt32(DatosFactura1[z, 10]);
                                }

                                int codigo_impto2N;
                                if (BD.consulta("SELECT COUNT(*) FROM tblImpuestos WHERE COD_IMP =" + Convert.ToInt32(DatosFactura1[z, 11]) + "") == "0")
                                {
                                    codigo_impto2N = 1; //“No existe el impuesto” + <código impto1>. Lo mismo aplica para <código impto2> DatosNotaDeVenta1[z, 11]
                                    escribe(3, "No existe el impuesto " + DatosFactura1[z, 11], nombre);

                                }
                                else
                                {
                                    codigo_impto2N = Convert.ToInt32(DatosFactura1[z, 11]);
                                }

                                string fechacad = CambioDeFecha(DatosFactura1[z, 21]);
                                BD.GuardaCambios("INSERT INTO tblFacturasRen(FOLIO_FAC, COD1_ART, CAN_ART, COD_UND, EQV_UND, PCIO_VEN, COD_MON, TIP_CAM, POR_DES, DECTO_ADI, COD1_IMP, COD2_IMP, IMP1_ART, IMP2_ART, IMP1_REG, IMP2_REG, IMPTO_IMP, NUM_MOV, IMP_SINDESC, DCTO_GRAL, PCIO_UNI, NUM_LOT, FEC_CAD) VALUES ('" + DatosFactura1[z, 0] + "', '" + articuloN + "', " + Convert.ToDecimal(DatosFactura1[z, 2]) + ", '" + unidadN + "', " + Convert.ToDecimal(DatosFactura1[z, 4]) + ",  " + verificaLongitud(DatosFactura1[z, 5]) + ", " + monedaN + ", " + Convert.ToDouble(DatosFactura1[z, 7]) + ", " + Convert.ToDecimal(DatosFactura1[z, 8]) + ", " + Convert.ToDecimal(DatosFactura1[z, 9]) + ", " + codigo_impto1N + ", " + codigo_impto2N + ", " + Convert.ToDecimal(DatosFactura1[z, 12]) + ", " + Convert.ToDecimal(DatosFactura1[z, 13]) + ", " + verificaLongitud(DatosFactura1[z, 14]) + ", " + verificaLongitud(DatosFactura1[z, 15]) + ", " + verificaLongitud(DatosFactura1[z, 16]) + ", " + Convert.ToInt64(DatosFactura1[z, 17]) + ", " + verificaLongitud(DatosFactura1[z, 18]) + ", " + verificaLongitud(DatosFactura1[z, 19]) + ", " + verificaLongitud(DatosFactura1[z, 20]) + ", '" + DatosFactura1[z, 22] + "', '" + fechacad + "')");
                                //NUM_MOV = consecutivo (incremento)
                            }
                        }

                    }

                }
            }


            //----------------------------------------------------------------------------------------------------------
            DatosFactura2 = new string[numerodedatos, 2];
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Folio_NV");
                foreach (XmlElement nodo in lista)
                {
                    int i = 0;

                    XmlNodeList folioNV = nodo.GetElementsByTagName("folioNV");
                    XmlNodeList folioFAC = nodo.GetElementsByTagName("folioFAC");

                    DatosFactura2[z, 0] = folioNV[i].InnerText;
                    DatosFactura2[z, 1] = folioFAC[i].InnerText;

                    for (int r = 0; foliosN.Length > r; r++)
                    {
                        if (foliosN[r] != "" && foliosN[r] != null)
                        {
                            if (foliosN[r] == DatosFactura2[z, 0])
                            {
                                BD.GuardaCambios("INSERT INTO tblnotasporfactura(FOLIO_NV, FOLIO_FACT) VALUES ('" + DatosFactura2[z, 0] + "', '" + DatosFactura2[z, 1] + "')");
                            }
                        }
                    }
                }
            }

            //----------------------------------------------------------------------------------------------------------
            DatosFactura3 = new string[numerodedatos, 4];
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Facturas[z]).GetElementsByTagName("Folio_Descripcion");
                foreach (XmlElement nodo in lista)
                {
                    int i = 0;

                    XmlNodeList FOLIO = nodo.GetElementsByTagName("FOLIO");
                    XmlNodeList COD_ART = nodo.GetElementsByTagName("COD_ART");
                    XmlNodeList NUM_MOV = nodo.GetElementsByTagName("NUM_MOV");
                    XmlNodeList DESC_ART = nodo.GetElementsByTagName("DESC_ART");

                    DatosFactura3[z, 0] = FOLIO[i].InnerText;
                    DatosFactura3[z, 1] = COD_ART[i].InnerText;
                    DatosFactura3[z, 2] = NUM_MOV[i].InnerText;
                    DatosFactura3[z, 3] = DESC_ART[i].InnerText;

                    for (int r = 0; foliosN.Length > r; r++)
                    {
                        if (foliosN[r] != "" && foliosN[r] != null)
                        {
                            if (foliosN[r] == DatosFactura3[z, 0])
                            {
                                int numero;
                                if (DatosFactura3[z, 2] != "" && DatosFactura3[z, 2] != null)
                                {
                                    numero = Convert.ToInt32(DatosFactura3[z, 2]);
                                }
                                else
                                {
                                    numero = 0;
                                }

                                BD.GuardaCambios("INSERT INTO tblFeDescripciones(FOLIO, COD_ART, NUM_MOV, DESC_ART) VALUES ('" + DatosFactura3[z, 0] + "', '" + DatosFactura3[z, 1] + "', " + numero + ", '" + DatosFactura3[z, 3] + "')");
                            }
                        }
                    }
                }
            }
            #endregion
            //-------------------------------------------------------------------------------------------------------------
            #region Pedidos
            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Pedidos", nombre);
            XmlNodeList Pedidos = xDoc.GetElementsByTagName("Pedidos");

            numerodedatos = Pedidos.Count;
            DatosPedidos1 = new string[numerodedatos, 31];
            cfn = 0;
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Pedidos[z]).GetElementsByTagName("Datos_Generales_Pedido");

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


                    DatosPedidos1[z, 0] = tipo_de_movimiento[i].InnerText;
                    DatosPedidos1[z, 1] = folio_pedido[i].InnerText;
                    DatosPedidos1[z, 2] = fecha_pedido[i].InnerText;
                    DatosPedidos1[z, 3] = hora_pedido[i].InnerText;
                    DatosPedidos1[z, 4] = tipo_pedido[i].InnerText;
                    DatosPedidos1[z, 5] = referencia_pedido[i].InnerText;
                    DatosPedidos1[z, 6] = docto_de_referencia[i].InnerText;
                    DatosPedidos1[z, 7] = cliente_pedido[i].InnerText;
                    DatosPedidos1[z, 8] = ruta_pedido[i].InnerText;
                    DatosPedidos1[z, 9] = vendedor_pedido[i].InnerText;
                    DatosPedidos1[z, 10] = almacen_pedido[i].InnerText;
                    DatosPedidos1[z, 11] = subtotal_pedido[i].InnerText;
                    DatosPedidos1[z, 12] = impuesto_pedido[i].InnerText;
                    DatosPedidos1[z, 13] = importe_exento_pedido[i].InnerText;
                    DatosPedidos1[z, 14] = total_pedido[i].InnerText;
                    DatosPedidos1[z, 15] = impuesto_int_pedido[i].InnerText;
                    DatosPedidos1[z, 16] = estatus_pedido[i].InnerText;
                    DatosPedidos1[z, 17] = vehiculo_pedido[i].InnerText;
                    DatosPedidos1[z, 18] = notas_pedido[i].InnerText;
                    DatosPedidos1[z, 19] = sucursal_pedido[i].InnerText;
                    DatosPedidos1[z, 20] = cambio_pedido[i].InnerText;

                    string rutadp;
                    if (BD.consulta("SELECT COUNT(*) FROM tblRutas WHERE COD_RUTA ='" + DatosPedidos1[z, 8] + "'") == "0")
                    {
                        rutadp = ""; //No existe la ruta” + <ruta pedido>
                        escribe(3, "No existe la ruta " + DatosPedidos1[z, 8], nombre);

                    }
                    else
                    {
                        rutadp = DatosPedidos1[z, 8];
                    }

                    string vendedorN;
                    if (BD.consulta("SELECT COUNT(*) FROM tblVendedores WHERE COD_VEN ='" + DatosPedidos1[z, 9] + "'") == "0")
                    {
                        vendedorN = "PISO"; //“No existe el vendedor” + <vendedor> DatosNotaDeVenta1[z, 9]
                        escribe(3, "No existe el vendedor " + DatosPedidos1[z, 9], nombre);
                    }
                    else
                    {
                        vendedorN = DatosPedidos1[z, 9];
                    }

                    string almacenN;
                    if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + DatosPedidos1[z, 10] + "'") == "0")
                    {
                        almacenN = funciones.LeerArchivoINI("FAST FOOD", "MERCANCIA", Application.StartupPath); //“No existe el almacen” + <almacen>. DatosNotaDeVenta1[z, 10]
                        escribe(3, "No existe el almacén " + DatosPedidos1[z, 10], nombre);
                    }
                    else
                    {
                        almacenN = DatosPedidos1[z, 10];
                    }

                    string camion;
                    if (DatosPedidos1[z, 17] != "")
                    {
                        if (BD.consulta("SELECT COUNT(*) FROM tblCamiones WHERE NUM_CAM ='" + DatosPedidos1[z, 17] + "'") == "0")
                        {
                            camion = ""; //“No existe el almacen” + <almacen>. DatosNotaDeVenta1[z, 17]
                            escribe(3, "No existe vehículo " + DatosPedidos1[z, 17], nombre);
                        }
                        else
                        {
                            camion = DatosPedidos1[z, 17];
                        }
                    }
                    else
                    {
                        camion = DatosPedidos1[z, 17];
                    }


                    if (BD.consulta("SELECT COUNT(*) FROM tblEncPedidos WHERE FOL_PED ='" + DatosPedidos1[z, 1] + "'") == "0")
                    {
                        if (BD.consulta("SELECT COUNT(*) FROM tblCatClientes WHERE COD_CLI ='" + DatosPedidos1[z, 7] + "'") == "0")
                        {
                            //Datos que no vienen en el XML:
                            //VEN_ENT = no se graba nada
                            //ENVIADO = 0
                            BD.GuardaCambios("INSERT INTO tblEncPedidos(FOL_PED, FEC_PED, HORA_DOC, TIPO_PED, FOL_REF, REF_PED, COD_CLI, COD_RUTA, COD_VEN, COD_ALM, SUB_PED, IVA_PED, IMPTO_IMPTOT, TOT_PED, IMPTO_INT, STS_PED, NUM_CAM, NOTA, COD_SUCU, CAM_PED, ENVIADO) VALUES ('" + DatosPedidos1[z, 1] + "', '" + CambioDeFecha(DatosPedidos1[z, 2]) + "', '" + DatosPedidos1[z, 3] + "', " + Convert.ToInt16(DatosPedidos1[z, 4]) + ", '" + DatosPedidos1[z, 5] + "', '" + DatosPedidos1[z, 6] + "', '" + DatosPedidos1[z, 7] + "', '" + rutadp + "', '" + vendedorN + "', '" + almacenN + "', " + Convert.ToDecimal(DatosPedidos1[z, 11]) + ", " + Convert.ToDecimal(DatosPedidos1[z, 12]) + ", " + Convert.ToDecimal(DatosPedidos1[z, 13]) + ", " + Convert.ToDecimal(DatosPedidos1[z, 14]) + ", " + Convert.ToDecimal(DatosPedidos1[z, 15]) + ", " + Convert.ToInt16(DatosPedidos1[z, 16]) + ", '" + camion + "', '" + DatosPedidos1[z, 18] + "', '" + DatosPedidos1[z, 19] + "', " + Convert.ToDecimal(DatosPedidos1[z, 20]) + ", 0)");
                            foliosN[cfn] = DatosNotaDeVenta[z, 1];
                            cfn = cfn + 1;
                        }
                        else
                        {
                            escribe(3, "No existe el cliente " + DatosPedidos1[z, 7] + " para el pedido " + DatosPedidos1[z, 1], nombre);
                            //No existe el cliente” + <cliente pedido> + “para el pedido” + <folio pedido>
                        }

                    }
                    else
                    {
                        //“Ya existe el Pedido” + FOL_PED.
                        escribe(3, "Ya existe el Pedido " + DatosPedidos1[z, 1], nombre);

                    }
                }
            }


            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Partidas_Pedidos", nombre);
            DatosPedidos2 = new string[numerodedatos, 15];
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Pedidos[z]).GetElementsByTagName("Partidas_Pedidos");
                numerodedatos1 = lista.Count;

                for (int y = 0; y < numerodedatos1; y++)
                {

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

                        DatosPedidos2[y, 0] = folio[i].InnerText;
                        DatosPedidos2[y, 1] = articulo[i].InnerText;
                        DatosPedidos2[y, 2] = cantidad[i].InnerText;
                        DatosPedidos2[y, 3] = unidad[i].InnerText;
                        DatosPedidos2[y, 4] = cantidad_hh[i].InnerText;
                        DatosPedidos2[y, 5] = cantidad_preventa[i].InnerText;
                        DatosPedidos2[y, 6] = cantidad_entregada[i].InnerText;
                        DatosPedidos2[y, 7] = backorder[i].InnerText;
                        DatosPedidos2[y, 8] = precio_venta[i].InnerText;
                        DatosPedidos2[y, 9] = porcentaje_descto[i].InnerText;
                        DatosPedidos2[y, 10] = precio_con_impto[i].InnerText;
                        DatosPedidos2[y, 11] = porcentaje_impto1[i].InnerText;
                        DatosPedidos2[y, 12] = porcentaje_impto2[i].InnerText;
                        DatosPedidos2[y, 13] = importe_exento[i].InnerText;
                        DatosPedidos2[y, 14] = estatus[i].InnerText;

                        for (int r = 0; foliosN.Length > r; r++)
                        {
                            if (foliosN[r] != "" && foliosN[r] != null)
                            {
                                if (foliosN[r] == DatosPedidos2[y, 0])
                                {

                                    string articuloN;
                                    if (BD.consulta("SELECT COUNT(*) FROM tblCatArticulos WHERE COD1_ART ='" + DatosPedidos2[y, 1] + "'") == "0")
                                    {
                                        articuloN = "DEPURADO"; //“No existe el artículo” + <articulo> DatosPedidos2[z, 1]
                                        escribe(3, "No existe el artículo " + DatosPedidos2[y, 1], nombre);
                                    }
                                    else
                                    {
                                        articuloN = DatosPedidos2[y, 1];
                                    }

                                    string unidadN;
                                    if (BD.consulta("SELECT COUNT(*) FROM tblUndCosPreArt WHERE COD1_ART ='" + articuloN + "' AND COD_UND ='" + DatosPedidos2[y, 3] + "'") == "0")
                                    {
                                        unidadN = "1";//“No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad> DatosPedidos2[z,6]
                                        escribe(3, "No coincide la equivalencia del articulo " + articuloN + " para la unidad " + DatosPedidos2[y, 6], nombre);
                                    }
                                    else
                                    {
                                        unidadN = DatosPedidos2[y, 3];
                                    }

                                    Decimal codigo_impto1N;
                                    codigo_impto1N = Convert.ToDecimal(DatosPedidos2[y, 11]);
                                    Decimal codigo_impto2N;
                                    codigo_impto2N = Convert.ToDecimal(DatosPedidos2[y, 12]);

                                    BD.GuardaCambios("INSERT INTO tblRenPedidos(FOL_PED, COD1_ART, CAN_CAR, COD_UND, CAN_HNH, CAN_PRE, CAN_ENT, CAN_BACK, PRE_ART, POR_DES, PCIO_UNI, IMP1_ART, IMP2_ART, IMPTO_IMP, STS_PED, COD_RUTA) VALUES ('" + DatosPedidos2[y, 0] + "', '" + articuloN + "', " + Convert.ToDecimal(DatosPedidos2[y, 2]) + ", '" + unidadN + "' , " + Convert.ToDecimal(DatosPedidos2[y, 4]) + ", " + Convert.ToDecimal(DatosPedidos2[y, 5]) + ", " + Convert.ToDecimal(DatosPedidos2[y, 6]) + ", " + Convert.ToDecimal(DatosPedidos2[y, 7]) + ", " + Convert.ToDecimal(DatosPedidos2[y, 8]) + ", " + Convert.ToDecimal(DatosPedidos2[y, 9]) + ", " + Convert.ToDecimal(DatosPedidos2[y, 10]) + ", " + codigo_impto1N + ", " + codigo_impto2N + " , " + Convert.ToDecimal(DatosPedidos2[y, 13]) + ", " + Convert.ToInt16(DatosPedidos2[y, 14]) + " , '" + BD.consulta("SELECT COD_RUTA FROM tblencpedidos WHERE FOL_PED = '" + DatosPedidos2[y, 0] + "'") + "')");
                                    //Datos que no vienen en el XML:
                                    //COD_RUTA = misma que en tblEncPedidos.
                                }
                            }

                        }

                    }
                }
            }
            #endregion
            //-------------------------------------------------------------------------------------------------------------
            #region Estructura_del_Paquete_de_Transacciones
            XmlNodeList Operaciones_de_caja = xDoc.GetElementsByTagName("Estructura_del_Paquete_de_Transacciones");

            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Operaciones_de_caja", nombre);
            numerodedatos = Operaciones_de_caja.Count;
            DatosAuxiliarCaja = new string[numerodedatos, 33];
            for (int x = 0; x < numerodedatos; x++)
            {
                lista = ((XmlElement)Operaciones_de_caja[x]).GetElementsByTagName("Operaciones_de_caja");

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

                    DatosAuxiliarCaja[x, 0] = tipo_de_movimiento[i].InnerText;
                    DatosAuxiliarCaja[x, 1] = caja[i].InnerText;
                    DatosAuxiliarCaja[x, 2] = turno[i].InnerText;
                    DatosAuxiliarCaja[x, 3] = concepto[i].InnerText;
                    DatosAuxiliarCaja[x, 4] = concepto_de_caja[i].InnerText;
                    DatosAuxiliarCaja[x, 5] = concepto_general[i].InnerText;
                    DatosAuxiliarCaja[x, 6] = folio[i].InnerText;
                    DatosAuxiliarCaja[x, 7] = folio_general[i].InnerText;
                    DatosAuxiliarCaja[x, 8] = referencia_general[i].InnerText;
                    DatosAuxiliarCaja[x, 9] = fecha[i].InnerText;
                    DatosAuxiliarCaja[x, 10] = hora[i].InnerText;
                    DatosAuxiliarCaja[x, 11] = referencia_pago[i].InnerText;
                    DatosAuxiliarCaja[x, 12] = referencia_adicional[i].InnerText;
                    DatosAuxiliarCaja[x, 13] = cliente[i].InnerText;
                    DatosAuxiliarCaja[x, 14] = forma_de_pago[i].InnerText;
                    DatosAuxiliarCaja[x, 15] = moneda[i].InnerText;
                    DatosAuxiliarCaja[x, 16] = tipo_de_cambio[i].InnerText;
                    DatosAuxiliarCaja[x, 17] = importe[i].InnerText;
                    DatosAuxiliarCaja[x, 18] = importe_MN[i].InnerText;
                    DatosAuxiliarCaja[x, 19] = cargo[i].InnerText;
                    DatosAuxiliarCaja[x, 20] = saldo[i].InnerText;
                    DatosAuxiliarCaja[x, 21] = usuarios[i].InnerText;
                    DatosAuxiliarCaja[x, 22] = autoriza[i].InnerText;
                    DatosAuxiliarCaja[x, 23] = corte_virtual[i].InnerText;
                    DatosAuxiliarCaja[x, 24] = corte_parcial[i].InnerText;
                    DatosAuxiliarCaja[x, 25] = corte_final[i].InnerText;
                    DatosAuxiliarCaja[x, 26] = contabilizado[i].InnerText;
                    DatosAuxiliarCaja[x, 27] = importe_cambio[i].InnerText;
                    DatosAuxiliarCaja[x, 28] = moneda_cambio[i].InnerText;
                    DatosAuxiliarCaja[x, 29] = tc_cambio[i].InnerText;
                    DatosAuxiliarCaja[x, 30] = notas[i].InnerText;
                    DatosAuxiliarCaja[x, 31] = sucursal[i].InnerText;
                    DatosAuxiliarCaja[x, 32] = cuenta_pago[i].InnerText;


                    if (BD.consulta("SELECT COUNT(*) FROM tblCatClientes WHERE COD_CLI ='" + DatosAuxiliarCaja[x, 13] + "'") == "0")
                    {
                        clienteNV1 = "PUBLIC";
                        escribe(3, "No existe el cliente " + DatosAuxiliarCaja[x, 13] + " para la NV " + DatosAuxiliarCaja[x, 6], nombre);
                    }//“No existe el cliente” + <clienteNV> + “para la NV” + <folio NV> DatosAuxiliarCaja[z, 13], DatosAuxiliarCaja[z, 6]

                    else { clienteNV1 = DatosAuxiliarCaja[x, 13]; }

                    int formaDC;
                    if (BD.consulta("SELECT COUNT(*) FROM tblFormasPago WHERE COD_FRP =" + Convert.ToInt32(DatosAuxiliarCaja[x, 14]) + "") == "0")
                    {
                        formaDC = 1;
                        escribe(3, "No existe la forma de pago " + DatosAuxiliarCaja[x, 14], nombre);
                    }//: “No existe la forma de pago” + <forma de pago>. DatosAuxiliarCaja[z, 14]
                    else { formaDC = Convert.ToInt32(DatosAuxiliarCaja[x, 14]); }

                    int monedaN;
                    if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(DatosAuxiliarCaja[x, 15]) + "") == "0")
                    {
                        monedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[z, 15]
                        escribe(3, "No existe la moneda " + DatosAuxiliarCaja[x, 15], nombre);
                    }
                    else
                    {
                        monedaN = Convert.ToInt32(DatosAuxiliarCaja[x, 15]);
                    }

                    Int16 TCmonedaN;
                    if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE TIP_CAM =" + Convert.ToDouble(DatosAuxiliarCaja[x, 28]) + "") == "0")
                    {
                        TCmonedaN = 1; //“No existe la moneda” + <moneda> + “de la nota de venta” + <folio> DatosNotaDeVenta1[z, 28]
                        escribe(3, "No existe el tipo de cambio " + DatosAuxiliarCaja[x, 28] + "de la nota de venta" + DatosAuxiliarCaja[x, 6], nombre);
                    }
                    else
                    {
                        TCmonedaN = Convert.ToInt16(DatosAuxiliarCaja[x, 28]);
                    }

                    string usuarioNV123;
                    if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosAuxiliarCaja[x, 21] + "'") == "0")
                    {
                        usuarioNV123 = "DEPURADO";//“No existe el usuario” + <usuario>  DatosFactura[z, 21], 
                        escribe(3, "No existe el usuario " + DatosAuxiliarCaja[x, 21], nombre);

                    }
                    else
                    {
                        usuarioNV123 = DatosAuxiliarCaja[x, 21];
                    }

                    string AUTusuarioNV123;

                    if (DatosAuxiliarCaja[x, 22] != "")
                    {
                        if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosAuxiliarCaja[x, 22] + "'") == "0")
                        {
                            AUTusuarioNV123 = "DEPURADO";//“No existe el usuario” + <autoriza>  DatosFactura[z, 22], 
                            escribe(3, "No existe el usuario " + DatosAuxiliarCaja[x, 22], nombre);

                        }
                        else
                        {
                            AUTusuarioNV123 = DatosAuxiliarCaja[x, 22];
                        }
                    }
                    else
                    {
                        AUTusuarioNV123 = DatosAuxiliarCaja[x, 22];
                    }


                    if (BD.consulta("SELECT COUNT(*) FROM tblCajas  WHERE COD_CAJ =" + Convert.ToInt32(DatosAuxiliarCaja[x, 1]) + "") == "0")
                    {
                        //“No existe la caja” + <caja>.
                        escribe(3, "No existe la caja " + DatosAuxiliarCaja[x, 1], nombre);

                    }
                    else
                    {
                        if (DatosAuxiliarCaja[x, 4] == "PSER")
                        {
                            if (BD.consulta("SELECT COUNT(*) FROM tblServicios WHERE COD_SER ='" + DatosAuxiliarCaja[x, 3] + "'") == "0")
                            {
                                //“No existe el servicio de caja” + <concepto> 
                                escribe(3, "No existe el servicio de caja " + DatosAuxiliarCaja[x, 3], nombre);
                            }
                            else
                            {
                                if (DatosAuxiliarCaja[x, 4] == "RCAJ" || DatosAuxiliarCaja[x, 4] == "IEFE")
                                {
                                    if (BD.consulta("SELECT COUNT(*) FROM tblConceptoIE WHERE COD_CIE ='" + DatosAuxiliarCaja[x, 3] + "'") == "0")
                                    {
                                        //“No existe el concepto de caja” + <concepto>. 
                                        escribe(3, "No existe el concepto de caja " + DatosAuxiliarCaja[x, 3], nombre);
                                    }
                                    else
                                    {
                                        BD.GuardaCambios("INSERT INTO tblAuxCaja(COD_CAJ, TUR_CAJ, CON_CEP, COD_CON, CON_GRL, REF_DOC, FOL_GRL, REF_GRL, FEC_DOC, HORA_DOC, REF_PAG, REF_ADI, COD_CLI, COD_FRP, COD_MON, TIP_CAM, IMP_EXT, IMP_MBA, IMP_CAR, SAL_DOC, COD_USU, USU_AUT, FOL_VIR, FOL_PAR, FOL_FIN, CONTAB, IMPE_CAMBIO, MON_CAMBIO, TC_CAMBIO, NOTAS, POR_CAR, ENVIADO, COD_SUCU, CTA_PAGO) VALUES (" + Convert.ToInt16(DatosAuxiliarCaja[x, 1]) + ", " + Convert.ToInt16(DatosAuxiliarCaja[x, 2]) + ", '" + DatosAuxiliarCaja[x, 3] + "', '" + DatosAuxiliarCaja[x, 4] + "', '" + DatosAuxiliarCaja[x, 5] + "', '" + DatosAuxiliarCaja[x, 6] + "', '" + DatosAuxiliarCaja[x, 7] + "', '" + DatosAuxiliarCaja[x, 8] + "', '" + CambioDeFecha(DatosAuxiliarCaja[x, 9]) + "', '" + DatosAuxiliarCaja[x, 10] + "', '" + DatosAuxiliarCaja[x, 11] + "', '" + DatosAuxiliarCaja[x, 12] + "', '" + clienteNV1 + "', " + formaDC + ", " + monedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[x, 16]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[x, 17]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[x, 18]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[x, 19]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[x, 20]) + ", '" + usuarioNV123 + "', '" + AUTusuarioNV123 + "', '" + DatosAuxiliarCaja[x, 23] + "', '" + DatosAuxiliarCaja[x, 24] + "', '" + DatosAuxiliarCaja[x, 25] + "', " + Convert.ToInt16(DatosAuxiliarCaja[x, 26]) + ", " + Convert.ToDecimal(DatosAuxiliarCaja[x, 27]) + ", " + TCmonedaN + ", " + Convert.ToDecimal(DatosAuxiliarCaja[x, 29]) + ", '" + DatosAuxiliarCaja[x, 30] + "', 0, 0, '" + DatosAuxiliarCaja[x, 31] + "', '" + DatosAuxiliarCaja[x, 32] + "')");
                                        //Datos que no vienen en el XML:
                                        //NUM_MOV = consecutivo (incremento)
                                        //POR_CAR = 0
                                        //FOL_COR = no se graba nada
                                        //ENVIADO = 0
                                    }

                                }
                            }

                        }
                    }

                }
            }
            #endregion
            //---------------------------------------------------------------------------------------------------------------
            #region Cartera
            XmlNodeList Cartera = xDoc.GetElementsByTagName("Cartera");
            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Cartera", nombre);
            cfn = 0;
            numerodedatos = Cartera.Count;
            DatosCartera = new string[numerodedatos, 23];
            for (int x = 0; x < numerodedatos; x++)
            {
                //lista = ((XmlElement)Cartera[x]).GetElementsByTagName("Cartera");

                foreach (XmlElement nodo in Cartera)
                {
                    int i = 0;

                    XmlNodeList tipo_de_movimiento = nodo.GetElementsByTagName("tipo_de_movimiento");
                    XmlNodeList folio = nodo.GetElementsByTagName("folio");
                    XmlNodeList folio_general = nodo.GetElementsByTagName("folio_general");
                    XmlNodeList concepto = nodo.GetElementsByTagName("concepto");
                    XmlNodeList concepto_del_documento = nodo.GetElementsByTagName("concepto_del_documento");
                    XmlNodeList concepto_general = nodo.GetElementsByTagName("concepto_general");
                    XmlNodeList clientes = nodo.GetElementsByTagName("clientes");
                    XmlNodeList fecha = nodo.GetElementsByTagName("fecha");
                    XmlNodeList hora = nodo.GetElementsByTagName("hora");
                    XmlNodeList fecha_registro = nodo.GetElementsByTagName("fecha_registro");
                    XmlNodeList usuario = nodo.GetElementsByTagName("usuario");
                    XmlNodeList estatus = nodo.GetElementsByTagName("estatus");
                    XmlNodeList notas = nodo.GetElementsByTagName("notas");
                    XmlNodeList importe = nodo.GetElementsByTagName("importe");
                    XmlNodeList porcentaje_impuesto = nodo.GetElementsByTagName("porcentaje_impuesto");
                    XmlNodeList importe_impuesto = nodo.GetElementsByTagName("importe_impuesto");
                    XmlNodeList saldo = nodo.GetElementsByTagName("saldo");
                    XmlNodeList plazo = nodo.GetElementsByTagName("plazo");
                    XmlNodeList caja = nodo.GetElementsByTagName("caja");
                    XmlNodeList sucursal = nodo.GetElementsByTagName("sucursal");
                    XmlNodeList contabilizado = nodo.GetElementsByTagName("contabilizado");
                    XmlNodeList folio_liq = nodo.GetElementsByTagName("folio_liq");

                    DatosCartera[x, 0] = tipo_de_movimiento[i].InnerText;
                    DatosCartera[x, 1] = folio[i].InnerText;
                    DatosCartera[x, 2] = folio_general[i].InnerText;
                    DatosCartera[x, 3] = concepto[i].InnerText;
                    DatosCartera[x, 4] = concepto_del_documento[i].InnerText;
                    DatosCartera[x, 5] = concepto_general[i].InnerText;
                    DatosCartera[x, 6] = clientes[i].InnerText;
                    DatosCartera[x, 7] = fecha[i].InnerText;
                    DatosCartera[x, 8] = hora[i].InnerText;
                    DatosCartera[x, 9] = fecha_registro[i].InnerText;
                    DatosCartera[x, 10] = usuario[i].InnerText;
                    DatosCartera[x, 11] = estatus[i].InnerText;
                    DatosCartera[x, 12] = notas[i].InnerText;
                    DatosCartera[x, 13] = importe[i].InnerText;
                    DatosCartera[x, 14] = porcentaje_impuesto[i].InnerText;
                    DatosCartera[x, 15] = importe_impuesto[i].InnerText;
                    DatosCartera[x, 16] = saldo[i].InnerText;
                    DatosCartera[x, 17] = plazo[i].InnerText;
                    DatosCartera[x, 18] = caja[i].InnerText;
                    DatosCartera[x, 19] = sucursal[i].InnerText;
                    DatosCartera[x, 20] = contabilizado[i].InnerText;
                    DatosCartera[x, 21] = folio_liq[i].InnerText;


                    string conceptoDF;
                    if (BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE COD_CON ='" + DatosCartera[x, 4] + "'") == "0")
                    {
                        conceptoDF = BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE TIP_MOV ='" + DatosCartera[x, 5] + "'");
                        //No existe el concepto” + <concepto del documento> + “se asignó” + TIP_MOV
                        escribe(3, "No existe el concepto " + DatosCartera[x, 4] + " se asignó " + conceptoDF, nombre);

                    }
                    else { conceptoDF = DatosCartera[x, 4]; }


                    if (BD.consulta("SELECT COUNT(*) FROM tblCatClientes WHERE COD_CLI ='" + DatosCartera[x, 6] + "'") == "0")
                    {
                        clienteNV1 = "PUBLIC";
                        escribe(3, "No existe el cliente " + DatosCartera[x, 6] + " para la FACTURA " + DatosCartera[x, 1], nombre);
                    }//“No existe el cliente” + <clienteNV> + “para la FACTURA + <folio NV> DatosCartera[z, 1], DatosCartera[z, 6]
                    else { clienteNV1 = DatosCartera[x, 6]; }


                    string usuarioNV123;
                    if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + DatosCartera[x, 10] + "'") == "0")
                    {
                        usuarioNV123 = "DEPURADO";//“No existe el usuario” + <usuario>  DatosFactura[z, 21], 
                        escribe(3, "No existe el cliente " + DatosCartera[x, 6] + " para la FACTURA " + DatosCartera[x, 1], nombre);
                    }
                    else
                    {
                        usuarioNV123 = DatosCartera[x, 10];
                    }


                    int caja321;
                    if (BD.consulta("SELECT COUNT(*) FROM tblCajas WHERE COD_CAJ =" + Convert.ToInt32(DatosCartera[x, 18]) + "") == "0")
                    {
                        caja321 = Convert.ToInt32(BD.consulta("SELECT COUNT(*) FROM tblCajas WHERE VEN_CAJ = 1 ")); //“No existe la caja de cobranza” + <caja> “se asignó 1
                        escribe(3, "No existe la caja de cobranza " + DatosCartera[x, 18] + " se asignó 1", nombre);
                    }
                    else
                    {
                        caja321 = Convert.ToInt32(DatosCartera[x, 18]);
                    }

                    if (BD.consulta("SELECT COUNT(*) FROM tblEncCargosAbonos WHERE FOL_DOC ='" + DatosCartera[x, 1] + "'") != "0")
                    {
                        //“Ya existe el movimiento en Cartera” + 3 + <folio>.DatosCartera[x, 1]
                        escribe(3, "Ya existe el movimiento en Cartera " + DatosCartera[x, 3] + " " + DatosCartera[x, 1], nombre);
                    }
                    else
                    {

                        if (BD.consulta("SELECT COUNT(*) FROM tblEncCargosAbonos WHERE FOL_GRL ='" + DatosCartera[x, 2] + "'") != "0")
                        {
                            //“Ya existe el movimiento en Cartera” + 3 + <folio>.DatosCartera[x, 2]
                            escribe(3, "Ya existe el movimiento en Cartera " + DatosCartera[x, 3] + " " + DatosCartera[x, 2], nombre);
                        }
                        else
                        {
                            string sentencia = "INSERT INTO tblEncCargosAbonos(FOL_DOC, FOL_GRL, COD_CON, CON_CEP, CON_GRL, COD_CLI, FEC_DOC, HORA_DOC, FEC_REG, COD_USU, COD_STS, NOTA, IMP_DOC, POR_IMP, IVA_DOC, SAL_DOC, PLA_PAG, COD_CAJ, COD_SUCU, CONTAB, FOL_LIQ, ENVIADO) VALUES ('" + DatosCartera[x, 1] + "', '" + DatosCartera[x, 2] + "', '" + DatosCartera[x, 3] + "', '" + conceptoDF + "', '" + DatosCartera[x, 5] + "', '" + clienteNV1 + "', '" + CambioDeFecha(DatosCartera[x, 7]) + "', '" + DatosCartera[x, 8] + "', '" + CambioDeFecha(DatosCartera[x, 9]) + "', '" + usuarioNV123 + "', " + Convert.ToInt16(DatosCartera[x, 11]) + ", '" + DatosCartera[x, 12] + "', " + Convert.ToDecimal(DatosCartera[x, 13]) + ", " + Convert.ToDecimal(DatosCartera[x, 14]) + ", " + Convert.ToDecimal(DatosCartera[x, 15]) + ", " + Convert.ToDecimal(DatosCartera[x, 16]) + ", " + Convert.ToInt64(DatosCartera[x, 17]) + ", " + caja321 + ", '" + DatosCartera[x, 19] + "', " + Convert.ToInt16(DatosCartera[x, 20]) + ", '" + DatosCartera[x, 21] + "', 0)";
                            //MessageBox.Show(sentencia + Environment.NewLine + clienteNV1);
                            BD.GuardaCambios(sentencia);

                            if (clienteNV1 != "PUBLIC")
                            {

                                decimal SaldoTotal;
                                string datototal;

                                datototal = BD.consulta("SELECT SAL_CLI FROM tblCatClientes WHERE COD_CLI = '" + clienteNV1 + "'");


                                SaldoTotal = Convert.ToDecimal(datototal);

                                if (DatosCartera[x, 5] == "CCLI")
                                {
                                    SaldoTotal = SaldoTotal + Convert.ToDecimal(DatosCartera[x, 13]);
                                }
                                else
                                {
                                    if (DatosCartera[x, 5] == "ACLI")
                                    {
                                        SaldoTotal = SaldoTotal - Convert.ToDecimal(DatosCartera[x, 13]);
                                    }
                                }

                                BD.GuardaCambios("UPDATE tblCatClientes SET SAL_CLI = " + SaldoTotal + " WHERE COD_CLI = '" + clienteNV1 + "'");
                            }
                            //Datos que no vienen en el XML:
                            //NUM_MOV = consecutivo (incremento)
                            //ENVIADO = 0
                            foliosN[cfn] = DatosCartera[x, 1];
                            cfn = cfn + 1;
                        }

                    }
                    x = x + 1;
                }
            }

            //--------------------------------------------------------------------------------------------
            int folioenc;
            folioenc = 0;
            for (int x = 0; x < numerodedatos; x++)
            {
                lista = ((XmlElement)Cartera[x]).GetElementsByTagName("Documentos_Afectados");

                numerodedatos1 = lista.Count;
                for (int y = 0; y < numerodedatos1; y++)
                {


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

                        DatosCartera1[y, 0] = folio_carter[i].InnerText;
                        DatosCartera1[y, 1] = folio_documento[i].InnerText;
                        DatosCartera1[y, 2] = folio_general[i].InnerText;
                        DatosCartera1[y, 3] = concepto[i].InnerText;
                        DatosCartera1[y, 4] = concepto_documento[i].InnerText;
                        DatosCartera1[y, 5] = concepto_general[i].InnerText;
                        DatosCartera1[y, 6] = importe_aplicado[i].InnerText;
                        DatosCartera1[y, 7] = saldo_del_renglón[i].InnerText;
                        DatosCartera1[y, 8] = total_del_documento[i].InnerText;
                        DatosCartera1[y, 9] = estatus[i].InnerText;


                        //for (int r = 0; r < cfn; r++)
                        //{
                        if (foliosN[folioenc] != "" && foliosN[folioenc] != null)
                        {
                            if (foliosN[folioenc] == DatosCartera1[y, 0])
                            {
                                string xyz;
                                Int64 qwe;
                                qwe = Convert.ToInt64(BD.BuscaRegistroConVariasCondiciones("SELECT NUM_MOV FROM tblEncCargosAbonos WHERE  FOL_DOC ='" + DatosCartera1[y, 0] + "'", BD.conexionPV));
                                xyz = "INSERT INTO tblRenCargosAbonos(FOL_DOC, FOL_REF, FOL_GRL, COD_CON, CON_CEP, CON_GRL, IMP_DOC, SAL_DOC, TOT_DOC, COD_STS, FEC_DOC, COD_CLI, NUM_MOV) VALUES ('" + DatosCartera1[y, 0] + "', '" + DatosCartera1[y, 1] + "', '" + DatosCartera1[y, 2] + "', '" + DatosCartera1[y, 3] + "', '" + DatosCartera1[y, 4] + "', '" + DatosCartera1[y, 5] + "', " + Convert.ToDecimal(DatosCartera1[y, 6]) + ", " + Convert.ToDecimal(DatosCartera1[y, 7]) + ", " + Convert.ToDecimal(DatosCartera1[y, 8]) + ", " + Convert.ToInt16(DatosCartera1[y, 9]) + ", '" + CambioDeFecha(DatosCartera[folioenc, 7]) + "', '" + DatosCartera[folioenc, 6] + "', " + qwe + ")";
                                BD.GuardaCambios(xyz);

                                y = y + 1;
                                //Datos que no vienen en el XML:
                                //FEC_DOC = al de encabezado que le corresponde
                                //COD CLI = al de encabezado que le corresponde
                                //NUM_MOV = el del encabezado que le corresponde
                            }
                        } y = y + 1;

                        //}

                    } folioenc = folioenc + 1;
                }
            }
            #endregion
            //--------------------------------------------------------------------------------------------------------------
            #region Entradas_y_Salida
            //MessageBox.Show("almacenes");
            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Entradas_y_Salida", nombre);
            XmlNodeList Entradas_y_Salida = xDoc.GetElementsByTagName("Entradas_y_Salida");


            numerodedatos = Entradas_y_Salida.Count;
            datos1 = new string[numerodedatos, 23];
            cfn = 0;

            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Entradas_y_Salida[z]).GetElementsByTagName("Datos_Generales_Almacen");

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

                    datos1[z, 0] = tipo_de_movimiento[i].InnerText;
                    datos1[z, 1] = folio[i].InnerText;
                    datos1[z, 2] = folio_general[i].InnerText;
                    datos1[z, 3] = almacen[i].InnerText;
                    datos1[z, 4] = concepto[i].InnerText;
                    datos1[z, 5] = concepto_almacen[i].InnerText;
                    datos1[z, 6] = concepto_general[i].InnerText;
                    datos1[z, 7] = fecha[i].InnerText;
                    datos1[z, 8] = fecha_registro[i].InnerText;
                    datos1[z, 9] = hora[i].InnerText;
                    datos1[z, 10] = moneda[i].InnerText;
                    datos1[z, 11] = tipo_de_cambio[i].InnerText;
                    datos1[z, 12] = renglones[i].InnerText;
                    datos1[z, 13] = cantidad_total[i].InnerText;
                    datos1[z, 14] = costo_total[i].InnerText;
                    datos1[z, 15] = estatus[i].InnerText;
                    datos1[z, 16] = almacen_de_referencia[i].InnerText;
                    datos1[z, 17] = referencia_adicional[i].InnerText;
                    datos1[z, 18] = referencia_traspaso[i].InnerText;
                    datos1[z, 19] = usuario[i].InnerText;
                    datos1[z, 20] = autoriza[i].InnerText;
                    datos1[z, 21] = sucursal[i].InnerText;
                    datos1[z, 22] = contabilizado[i].InnerText;

                    string conceptoDF;
                    if (BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE COD_CON ='" + datos1[z, 5] + "'") == "0")
                    {
                        conceptoDF = BD.consulta("SELECT COUNT(*) FROM tblconceptos WHERE TIP_MOV ='" + datos1[z, 6] + "'");
                        //No existe el concepto” + <concepto del documento> + “se asignó” + TIP_MOV
                        escribe(3, "No existe el concepto " + datos1[z, 5] + " se asignó " + conceptoDF, nombre);

                    }
                    else
                    {
                        conceptoDF = datos1[z, 5];
                    }

                    int monedaN;
                    if (BD.consulta("SELECT COUNT(*) FROM tblMonedas WHERE COD_MON =" + Convert.ToInt32(datos1[z, 10]) + "") == "0")
                    {
                        monedaN = 1; //“No existe la moneda” + <moneda> DatosNotaDeVenta1[z, 10]
                        escribe(3, "No existe la moneda " + datos1[z, 10], nombre);

                    }
                    else
                    {
                        monedaN = Convert.ToInt32(datos1[z, 10]);
                    }

                    string codigoformaNV;
                    if (BD.consulta("SELECT COUNT(*) FROM tblUsuarios WHERE COD_USU ='" + datos1[z, 19] + "'") == "0")
                    {
                        codigoformaNV = "DEPURADO"; //“No existe la forma de pago” + <código forma>.
                        escribe(3, "No existe el usuario " + datos1[z, 19], nombre);
                    }
                    else
                    {
                        codigoformaNV = datos1[z, 19];
                    }


                    if (BD.consulta("SELECT COUNT(*) FROM tblGralAlmacen WHERE REF_MOV ='" + datos1[z, 1] + "'") != "0")
                    {
                        //“Ya existe el movimiento de almacén” + <concepto> + REF_MOV.DatosCartera[x, 1]
                        escribe(3, "Ya existe el movimiento de almacén " + datos1[z, 1], nombre);

                    }
                    else
                    {
                        if (BD.consulta("SELECT COUNT(*) FROM tblGralAlmacen WHERE FOL_GRL ='" + datos1[z, 2] + "'") != "0")
                        {
                            //“Ya existe el movimiento de almacén” + <concepto> + REF_MOV
                            escribe(3, "Ya existe el movimiento de almacén " + datos1[z, 2], nombre);

                        }
                        else
                        {
                            if (datos1[z, 3] != "")
                            {
                                if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + datos1[z, 3] + "'") == "0")
                                {
                                    //“No existe el almacen” + <almacen> + “no se agregó el movimiento”. Aplica lo mismo para <almacen de referencia>
                                    escribe(3, "No existe el almacen " + datos1[z, 3], nombre);
                                }
                                else
                                {

                                    BD.GuardaCambios("INSERT INTO tblGralAlmacen(REF_MOV, FOL_GRL, COD_ALM, CON_CEP, COD_CON, CON_GRL, FEC_MOV, FEC_REG, HORA_MOV, COD_MON, TIP_CAM, NUM_REN, SUM_CAN, COS_TOT, COD_STS, ALM_REF, REF_ADI, REF_TRA, COD_USU, USU_AUT, SUC_REF, CONTAB, ENVIADO ) VALUES ('" + datos1[z, 1] + "', '" + datos1[z, 2] + "', '" + datos1[z, 3] + "', '" + datos1[z, 4] + "', '" + conceptoDF + "', '" + datos1[z, 6] + "', '" + CambioDeFecha(datos1[z, 7]) + "', '" + CambioDeFecha(datos1[z, 8]) + "', '" + datos1[z, 9] + "', " + monedaN + ", " + Convert.ToDecimal(datos1[z, 11]) + ", " + Convert.ToInt64(datos1[z, 12]) + ", " + verificaLongitud(datos1[z, 13]) + ", " + verificaLongitud(datos1[z, 14]) + ", " + Convert.ToInt32(datos1[z, 15]) + ", '" + datos1[z, 16] + "', '" + datos1[z, 17] + "', '" + datos1[z, 18] + "', '" + codigoformaNV + "', '" + datos1[z, 20] + "', '" + datos1[z, 21] + "', " + Convert.ToInt32(datos1[z, 22]) + ", 0)");

                                    //Datos que no vienen en el XML:
                                    //NUM_MOV = consecutivo (incremento)
                                    //ENVIADO = 0


                                    FOCATI[cfn, 0] = datos1[z, 1];//folio
                                    FOCATI[cfn, 1] = datos1[z, 3];//Codigo Almacen
                                    FOCATI[cfn, 2] = datos1[z, 6];//Tipo de Movimiento
                                    cfn = cfn + 1;
                                }
                            }
                            else
                            {

                                BD.GuardaCambios("INSERT INTO tblGralAlmacen(REF_MOV, FOL_GRL, COD_ALM, CON_CEP, COD_CON, CON_GRL, FEC_MOV, FEC_REG, HORA_MOV, COD_MON, TIP_CAM, NUM_REN, SUM_CAN, COS_TOT, COD_STS, ALM_REF, REF_ADI, REF_TRA, COD_USU, USU_AUT, SUC_REF, CONTAB, ENVIADO ) VALUES ('" + datos1[z, 1] + "', '" + datos1[z, 2] + "', '" + datos1[z, 3] + "', '" + datos1[z, 4] + "', '" + conceptoDF + "', '" + datos1[z, 6] + "', '" + CambioDeFecha(datos1[z, 7]) + "', '" + CambioDeFecha(datos1[z, 8]) + "', '" + datos1[z, 9] + "', " + monedaN + ", " + Convert.ToDecimal(datos1[z, 11]) + ", " + Convert.ToInt64(datos1[z, 12]) + ", " + verificaLongitud(datos1[z, 13]) + ", " + verificaLongitud(datos1[z, 14]) + ", " + Convert.ToInt32(datos1[z, 15]) + ", '" + datos1[z, 16] + "', '" + datos1[z, 17] + "', '" + datos1[z, 18] + "', '" + codigoformaNV + "', '" + datos1[z, 20] + "', '" + datos1[z, 21] + "', " + Convert.ToInt32(datos1[z, 22]) + ", 0)");

                                //Datos que no vienen en el XML:
                                //NUM_MOV = consecutivo (incremento)
                                //ENVIADO = 0
                                FOCATI[cfn, 0] = datos1[z, 1];//folio
                                FOCATI[cfn, 1] = datos1[z, 3];//Codigo Almacen
                                FOCATI[cfn, 2] = datos1[z, 6];//Tipo de Movimiento
                                cfn = cfn + 1;
                            }
                        }
                    }
                }
            }

            //------------------------------------------------------------------------------------------

            escribe(2, "", nombre);
            escribe(2, "", nombre);
            escribe(1, "Renglones_Almacen", nombre);
            folioenc = 0;
            bool banderaRenglones = false;
            for (int z = 0; z < numerodedatos; z++)
            {
                lista = ((XmlElement)Entradas_y_Salida[z]).GetElementsByTagName("Renglones_Almacen");
                numerodedatos1 = lista.Count;

                for (int y = 0; y < numerodedatos1; y++)
                {


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


                        datos[y, 0] = folio[i].InnerText;
                        datos[y, 1] = almacen[i].InnerText;
                        datos[y, 2] = almacen_de_referencia[i].InnerText;
                        datos[y, 3] = articulo[i].InnerText;
                        datos[y, 4] = cantidad[i].InnerText;
                        datos[y, 5] = unidad[i].InnerText;
                        datos[y, 6] = equivalencia[i].InnerText;
                        datos[y, 7] = costo_unitario[i].InnerText;
                        datos[y, 8] = lote[i].InnerText;
                        datos[y, 9] = fecha_de_caducidad[i].InnerText;
                        datos[y, 10] = usuario[i].InnerText;
                        datos[y, 11] = tipo_de_cambio[i].InnerText;


                        //for (int r = 0; foliosN.Length > r; r++)
                        //{
                        if (FOCATI[folioenc, 0] != "" && FOCATI[folioenc, 0] != null)
                        {


                            if (FOCATI[folioenc, 0] == datos[y, 0])
                            {
                                banderaRenglones = true;
                                string codigoAlmacen;
                                if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + datos[y, 1] + "'") == "0")
                                {
                                    codigoAlmacen = FOCATI[folioenc, 1];
                                    //“No existe el almacen” + <almacen> + “no se agregó el movimiento”                            escribe(1, "Ya existe el movimiento de almacén " + datos1[z, 2], nombre);
                                    escribe(3, "No existe el almacen " + datos[y, 1], nombre);

                                }
                                else
                                {
                                    codigoAlmacen = datos[y, 1];
                                }

                                string codigoformaNV1 = "";

                                if (datos[y, 2].Length > 0)//corregido 16-abr-2013 .. no existía validación
                                {
                                    if (BD.consulta("SELECT COUNT(*) FROM tblCatAlmacenes WHERE COD_ALM ='" + datos[y, 2] + "'") == "0") //corregido 16-abr-2013 ... La validación tenía != en lugar de ==
                                    {
                                        codigoformaNV1 = BD.consulta("SELECT COD_ALM FROM tblGralAlmacen WHERE REF_MOV ='" + datos[y, 0] + "'");
                                        //“No existe el almacen” + <almacen> + “no se agregó el movimiento”
                                        escribe(3, "No existe el almacen " + datos[y, 2], nombre);
                                    }
                                    else
                                    {
                                        codigoformaNV1 = datos[y, 2];
                                    }
                                }

                                string unidadN;
                                if (BD.consulta("SELECT COUNT(*) FROM tblUndCosPreArt WHERE COD1_ART ='" + datos[y, 3] + "' AND COD_UND ='" + datos[y, 5] + "'") == "0")
                                {
                                    unidadN = "1";//: “No coincide la equivalencia del articulo” + <articulo> + “para la unidad” + <unidad>
                                    escribe(3, "No coincide la equivalencia del articulo " + datos[y, 3], nombre);

                                }
                                else
                                {
                                    unidadN = datos[y, 5];
                                }


                                string codigoformaNV123;
                                if (BD.consulta("SELECT COUNT(COD_USU) FROM tblUsuarios WHERE COD_USU = '" + datos[0, 10] + "'") == "0")
                                {
                                    codigoformaNV123 = "DEPURADO"; //“No existe la forma de pago” + <código forma>.
                                    escribe(3, "No existe el usuario " + datos[y, 10], nombre);
                                }
                                else
                                {
                                    codigoformaNV123 = datos[0, 10];
                                }
                                string abc = "", abc2 = "", abc3 = "";
                                DataTable datoss = BD.ObtieneDatosParaDataTableH1("SELECT NUM_MOV, FOL_GRL, SUC_REF  FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[y, 0] + "'", BD.conexionPV);
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
                                //abc = AC_Consultas.BuscaRegistroConVariasCondicionesH1("SELECT NUM_MOV FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[y, 0] + "'", AC_General.conexionPVH1);
                                //abc2 = AC_Consultas.BuscaRegistroConVariasCondicionesH1("SELECT FOL_GRL  FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[y, 0] + "'", AC_General.conexionPVH1);
                                //abc3 = AC_Consultas.BuscaRegistroConVariasCondicionesH1("SELECT SUC_REF FROM tblGralAlmacen WHERE  REF_MOV ='" + datos[y, 0] + "'", AC_General.conexionPVH1);
                                Int64 ab1;
                                decimal cEqvBase;
                                string cnn;
                                cEqvBase = 0;
                                if (abc.Length > 0)
                                    ab1 = Convert.ToInt64(abc);
                                else
                                    ab1 = 0;
                                BD.GuardaCambios("INSERT INTO tblRenAlmacen(REF_MOV, COD_ALM, ALM_REF, COD1_ART, CAN_REN, COD_UND, EQV_UND, COS_UNI, NUM_LOT, FEC_CAD, COD_USU, TC_ART, NUM_MOV, FOL_GRL, SUC_REF) VALUES ('" + datos[y, 0] + "', '" + codigoAlmacen + "', '" + codigoformaNV1 + "', '" + datos[y, 3] + "', " + verificaLongitud(datos[y, 4]) + ", '" + unidadN + "', " + Convert.ToDecimal(datos[y, 6]) + ", " + verificaLongitud(datos[y, 7]) + ", '" + datos[y, 8] + "', '" + CambioDeFecha(datos[y, 9]) + "',  '" + codigoformaNV123 + "', " + Convert.ToDecimal(datos[y, 11]) + ", " + ab1 + ", '" + abc2 + "', '" + abc3 + "')");

                                string almacenOficial;
                                decimal cantidadArticulo;

                                cantidadArticulo = Convert.ToDecimal(datos[y, 4]) * Convert.ToDecimal(datos[y, 6]);

                                almacenOficial = FOCATI[folioenc, 1];

                                if (almacenOficial == "" || almacenOficial == null || almacenOficial != codigoAlmacen)
                                {
                                    almacenOficial = codigoAlmacen;
                                }

                                string guardaCatArticulo = "", guardaExoPorAlmcen = "";


                                cCosPEPS = 0;
                                cnn = cnnxion;// AC_General.conexionPV.ConnectionString.ToString();
                                cEqvBase = Convert.ToDecimal(datos[y, 4]) * Convert.ToDecimal(datos[y, 6]);

                                if (FOCATI[folioenc, 2] == "EALM")
                                {
                                    objCostos.GrabarCapaDeCostos(ref cnn, datos[y, 0], "'" + DateTime.Now.ToString("yyyyMMdd") + "'", "EALM", datos[y, 3], Convert.ToDecimal(datos[y, 4]), unidadN, Convert.ToDecimal(datos[y, 6]), Convert.ToDecimal(datos[y, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[y, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'");

                                    guardaCatArticulo = "UPDATE tblCatArticulos Set EXI_ACT = EXI_ACT + " + cantidadArticulo + " WHERE COD1_ART = '" + datos[y, 3] + "'";
                                    guardaExoPorAlmcen = "UPDATE tblExiPorAlmacen Set EXI_ALM = EXI_ALM + " + cantidadArticulo + " WHERE COD1_ART = '" + datos[y, 3] + "' AND COD_ALM = '" + almacenOficial + "'";
                                    BD.GuardaCambios(guardaCatArticulo);
                                    BD.GuardaCambios(guardaExoPorAlmcen);
                                    objCostos.CalCostoPromedioNET(datos[y, 3], cnn, "'" + DateTime.Now.ToString("yyyyMMdd") + "'");
                                }
                                else
                                {
                                    if (FOCATI[folioenc, 2] == "SALM")
                                    {
                                        //20150325 se agrego la validaion de longitud a precio unitario datos[y, 7]
                                        //objCostos.GrabarCapaDeCostos(ref cnn, datos[y, 0], "'"+DateTime.Now.ToString("yyyyMMdd")+"'", "SALM", datos[y, 3], Convert.ToDecimal(datos[y, 4]), unidadN, Convert.ToDecimal(datos[y, 6]), Convert.ToDecimal(datos[y, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[y, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'");
                                        objCostos.GrabarCapaDeCostos(ref cnn, datos[y, 0], "'" + DateTime.Now.ToString("yyyyMMdd") + "'", "SALM", datos[y, 3], Convert.ToDecimal(datos[y, 4]), unidadN, Convert.ToDecimal(datos[y, 6]), verificaLongitud(datos[y, 7]), Convert.ToInt16(1), Convert.ToDecimal(datos[y, 11]), codigoAlmacen, 0, cEqvBase, "", "'18000101'");
                                        objCostos.SalidaPromedioNET(codigoAlmacen, datos[y, 0], ref cCosPEPS, datos[y, 3], cnn);

                                        guardaCatArticulo = "UPDATE tblCatArticulos Set EXI_ACT = EXI_ACT - " + cantidadArticulo + " WHERE COD1_ART = '" + datos[y, 3] + "'";
                                        guardaExoPorAlmcen = "UPDATE tblExiPorAlmacen Set EXI_ALM = EXI_ALM - " + cantidadArticulo + " WHERE COD1_ART = '" + datos[y, 3] + "' AND COD_ALM = '" + almacenOficial + "'";
                                        BD.GuardaCambios(guardaCatArticulo);
                                        BD.GuardaCambios(guardaExoPorAlmcen);
                                    }
                                }
                            }
                            else
                            {
                                banderaRenglones = false;
                            }
                            y = y + 1;

                        }

                        //}

                    }

                } if (banderaRenglones == true) { folioenc = folioenc + 1; }
            }

            #endregion
            escribe(1, "Fin de lectura - " + DateTime.Now.ToString(), nombre);
            mensajeSentencia = "";
            //}
            //finally
            //{
            #region destruye arreglos
            ResizeArray(ref DatosNotaDeVenta, 1);
            ResizeArray(ref DatosNotaDeVenta, 1);
            ResizeArray(ref DatosNotaDeVenta1, 1);
            ResizeArray(ref  DatosNotaDeVenta2, 1);

            ResizeArray(ref DatosFactura, 1);
            ResizeArray(ref DatosFactura1, 1);
            ResizeArray(ref DatosFactura2, 1);
            ResizeArray(ref DatosFactura3, 1);

            ResizeArray(ref DatosPedidos1, 1);
            ResizeArray(ref DatosPedidos2, 1);

            ResizeArray(ref DatosAuxiliarCaja, 1);

            ResizeArray(ref DatosCartera, 1);
            ResizeArray(ref DatosCartera1, 1);

            ResizeArray(ref datos, 1);
            ResizeArray(ref datos1, 1);

            Array.Resize(ref foliosN, 0);
            ResizeArray(ref FOCATI, 1);
            #endregion
            mensajeSentencia = "---" + BD.mensajeSententencia;
            //}

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
