using System.Drawing;
using System.Drawing.Printing;

namespace MVCVentas.Models
{
    public class Linea
    {
        public string texto { get; set; }

        public int tamano { get; set; }

        public string fuente { get; set; }

        public FontStyle estilo { get; set; }
    }

    public class Printer
    {
        public string? nombreImpresora { get; set; }

        public int countLineas()
        {
            return this.lineas.Count;
        }

        public void agregarLinea(string texto, int size = 7, string fuente = "Consolas", FontStyle fontStyle = FontStyle.Regular)
        {
            lineas.Add(new Linea { texto = texto, tamano = size, fuente = fuente, estilo = fontStyle });
        }

        public void agregarLineaConExtremos(string txtIzquierda, string txtDerecha, int size = 7, string fuente = "Consolas")
        {
            lineas.Add(new Linea { texto = $"{txtIzquierda,-25}{txtDerecha,30}", tamano = size, fuente = fuente, estilo = FontStyle.Regular });
        }

        public void agregarLineaNegrita(string texto, int size = 7, string fuente = "Consolas")
        {
            lineas.Add(new Linea { texto = texto, tamano = size, fuente = fuente, estilo = FontStyle.Bold });
        }

        public void agregarLineaEnBlanco(int size = 7)
        {
            this.agregarLinea(" ", size);
        }

        List<Linea> lineas = new List<Linea>();

        public List<Linea> ObtenerLineas()
        {
            return this.lineas;
        }

        public Printer()
        {

        }

        public Printer(string nombreImpresora)
        {
            this.nombreImpresora = nombreImpresora;
        }

        public void Imprimir()
        {
            PrintDocument p = new PrintDocument();

            p.PrintController = new StandardPrintController();

            if(!String.IsNullOrEmpty(nombreImpresora))
            {
                p.PrinterSettings = new PrinterSettings()
                {
                    PrinterName = nombreImpresora
                };
            }

            float alturaMaxDoc = 0;

            p.PrintPage += delegate (object sender1, PrintPageEventArgs e)
            {
                float cursor = 0;

                foreach (var linea in lineas)
                {
                    int size = linea.tamano;
                    string texto = linea.texto;
                    Font fuente = new Font(linea.fuente, size, linea.estilo);

                    float anchoMaximo = e.PageBounds.Width;

                    SizeF sizeText = e.Graphics.MeasureString(texto, fuente, (int)anchoMaximo);

                    alturaMaxDoc += sizeText.Height;

                    float x = 0; // Alineación a la izquierda por defecto

                    float alturaMax = sizeText.Height;

                    e.Graphics.DrawString(texto, fuente,
                        new SolidBrush(Color.Black),
                        new RectangleF(x, cursor,
                            anchoMaximo,
                            alturaMax));

                    cursor += alturaMax;
                }
            };

            // Asignar la altura total al tamaño de papel
            p.DefaultPageSettings.PaperSize = new PaperSize("Custom", (int)(80 * p.DefaultPageSettings.PrinterResolution.X / 25.4), (int)alturaMaxDoc);

            p.Print();
        }
    }
}
