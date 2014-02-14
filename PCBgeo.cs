using System;
using System.Collections;
using System.Collections.Generic;
using Clipper;

/// <summary>
/// Crea y define las geometrías apartir de aperturas
/// </summary>
namespace PCBgeo
{
    using TPolygon = List<TDoublePoint>; // coordenadas del poligono
    using TPolyPolygon = List<List<TDoublePoint>>;// lista de poligonos
    /// <summary>
    /// Crea y define las geometrías indicada por la Apertura
    /// </summary>
    public abstract class Apertura
    {
        protected int _dcode;
        protected TDoublePoint _centro;
        protected TPolygon _poligono;
        private Char _tipo;

        /// <summary>
        /// Crea un poligono con la forma de la apertura
        /// </summary>
        public abstract TPolygon Flash();

        /// <summary>
        /// Crea un poligono con la forma de la apertura
        /// </summary>
        /// <param name="cen">Centro del Path</param>
        /// <returns></returns>
        public abstract TPolygon Flash(TDoublePoint cen);

        /// <summary>
        /// Obtiene el código D asignado a la Apertura.
        /// </summary>
        public int DCode
        {
          get {  return _dcode;}
        }

        /// <summary>
        /// Obtiene el tipo de Apertura.
        /// </summary>
        protected Char Tipo
        {
            get { return _tipo; }
        }
    }

    /// <summary>
    /// Define la Apertura Circular usada por Gerber
    /// error % ((pi)-SEGMENTO*sin(pi/SEGMENTO))/(pi)
    /// </summary>
    public class AperturaCircular: Apertura
    {
        private double _diametro;
        private static int _SEGMENTO = 32;
        Char _tipo= 'C';

        /// <summary>
        /// Obtiene o establece la cantidad de segmentos usados para la aproximación 
        /// de la circunferencia
        /// </summary>
        public static int SEGMENTO
        {
            get { return AperturaCircular._SEGMENTO; }
            set { AperturaCircular._SEGMENTO = value; }
        }
        /// <summary>
        /// Obtiene el valor del ángulo de aproximación
        /// </summary>
        public static double SECTOR
        {
            get { return 2.0 * Math.PI /_SEGMENTO; }
        }
        /// <summary>
        /// Obtiene el diámetro de la apertura círculo.
        /// </summary>
        public double Diametro
        {
            get
            {
                return _diametro;
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase PCBgeo.AperturaCircular, con el codigo D y diametro indicado
        /// ubicado en el origen de coordenadas.
        /// </summary>
        /// <param name="code">Numero del codigo D</param>
        /// <param name="dia">Diametro de la apertura circular</param>
        public AperturaCircular(int code, double dia)
        {
            _dcode = code;
            _diametro = dia;
            _centro = new TDoublePoint(0, 0); 
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase PCBgeo.AperturaCircular, con el codigo D, diametro y 
        /// punto de ubicación del centro indicado.
        /// </summary>
        /// <param name="code">Codigo D</param>
        /// <param name="dia">Diametro de la apertura circular</param>
        /// <param name="cen">Punto de ubicación del centro</param>
        public AperturaCircular(int code, double dia, TDoublePoint cen)
        {
            _dcode = code;
            _diametro = dia;
            _centro = new TDoublePoint(cen.X, cen.Y);
        }

        /// <summary>
        /// Devuelve un TPolygon que representa la forma de la apertura círculo, con los paramentros del contructor.
        /// </summary>
        override
        public TPolygon Flash()
        {
            _poligono = HCirculo(_centro, Diametro, 0.0, 360.0f);
            return _poligono;
        }

       /// <summary>
        /// Devuelve un TPolygon que representa la forma de la apertura círculo ubicada en el nuevo punto centro.
       /// </summary>
       /// <param name="cen">Nuevo punto de ubicación de la aperura</param>
       /// <returns></returns>
        override
        public TPolygon Flash(TDoublePoint cen)
        {
            _centro = new TDoublePoint(cen.X, cen.Y);
            return this.Flash();
        }

        /// <summary>
        /// Crea Círculo con las dimensiones indicadas.
        /// </summary>
        /// <param name="Cent">Centro del círculo</param>
        /// <param name="dia">Diámetro del círculo</param>
        /// <param name="angInicio">Angulo de inicio en grados</param>
        /// <param name="desarrollo">Arco del círculo en grados</param>
        /// <returns></returns>
        public TPolygon HCirculo(TDoublePoint Cent, double dia, double angInicio, double desarrollo)
        {
            TPolygon _poligono = new TPolygon();

            double x, y;
            double Angulo = angInicio*Math.PI/180;

            for (int j = 0; j <= ((int)(SEGMENTO*desarrollo/360.0f)); j++)
            {
                x = (dia / 2) * Math.Cos(Angulo);
                y = (dia / 2) * Math.Sin(Angulo);
                _poligono.Add(new TDoublePoint(x + Cent.X, y + Cent.Y));
                Angulo += SECTOR;
            }

            return _poligono;
        }
        /// <summary>
        /// Crea trazo desde el punto inicial hasta el punto final indicado.
        /// </summary>
        /// <param name="Inicio">Punto inicial</param>
        /// <param name="Fin">Punto final</param>
        /// <returns></returns>
        public TPolygon Trazo(TDoublePoint Inicio, TDoublePoint Fin)
        {
            //    ___c___
            //   /       \
            // d(         )b
            //   \_______/
            //       a
            TPolygon geometria = new TPolygon();
            double r = Diametro / 2;

            //a_linea
            TDoublePoint a_Inicio;
            TDoublePoint a_Fin;
            double Angulo;
            //angulo de la segmento de pista menos -90º para calcular el punyo inical de a_linea 
            Angulo = (double)Math.Atan2(Fin.Y - Inicio.Y, Fin.X - Inicio.X) - (Math.PI / 2.0F);
            a_Inicio = new TDoublePoint((double)((r * Math.Cos(Angulo)) + Inicio.X), (double)(r * Math.Sin(Angulo) + Inicio.Y));
            a_Fin = new TDoublePoint((double)(r * Math.Cos(Angulo)) + Fin.X, (double)(r * Math.Sin(Angulo) + Fin.Y));
            geometria.Add(a_Inicio);
            geometria.Add(a_Fin);

            //b_arco
            double b_InicioAngulo;

            b_InicioAngulo = (double)(Angulo * 180F / Math.PI);
            geometria.AddRange(HCirculo(Fin, Diametro, b_InicioAngulo, 180F));
                        
            //c_linea
            TDoublePoint c_Inicio;
            TDoublePoint c_Fin;
            Angulo = Angulo + Math.PI; //angulo inicial + 180º

            c_Inicio = new TDoublePoint((double)(r * Math.Cos(Angulo)) + Fin.X, (double)(r * Math.Sin(Angulo) + Fin.Y));
            c_Fin = new TDoublePoint((double)(r * Math.Cos(Angulo)) + Inicio.X, (double)(r * Math.Sin(Angulo) + Inicio.Y));
             geometria.Add(c_Inicio);
            geometria.Add(c_Fin);


            //d_arco
            double d_InicioAngulo;

            d_InicioAngulo = b_InicioAngulo + 180.0F;
            geometria.AddRange(HCirculo(Inicio, Diametro, d_InicioAngulo, 180.0F));

            return geometria;
        }
    }
    

   /// <summary>
   /// Define la apertura Oblonga usadas por Gerber
   /// </summary>
    public class AperturaOblonga:Apertura 
    {
		double _altura = 0.0F; // crea e inicializa el alto;
		double _ancho = 0.0F; // crea e inicializa el ancho
        Char _tipo = 'O';
        
        /// <summary>
        /// Obtiene el alto del oblongo
        /// </summary>
        public double Altura
        {
            get { return _altura; }
            set { _altura = value; }
        }

        /// <summary>
        /// Obtiene el ancho del oblongo
        /// </summary>
        public double Ancho
        {
            get { return _ancho; }
            set { _ancho = value; }
        }
		/// <summary>
        /// Crea un oblongo con las dimensiones indicadas.
        /// </summary>
        /// <param name="code">Código de la apertura</param>
        /// <param name="anc">Ancho del oblongo </param>
        /// <param name="alt">Altura del oblongo </param>
        public AperturaOblonga(int code, double anc, double alt)
        {
            _altura = alt;
            _ancho = anc;
            _dcode = code;
            _centro = new TDoublePoint(0, 0);
        }

       /// <summary>
        /// Crea un oblongo con las dimensiones indicadas.
        /// </summary>
        /// <param name="code">Código de la apertura</param>
        /// <param name="anc">Ancho del oblongo</param>
        /// <param name="alt">Altura del oblongo </param>
        /// <param name="cen">Centro del oblongo</param>
        public AperturaOblonga(int code, double anc, double alt, TDoublePoint cen)
        {

            _altura = alt;
            _ancho = anc;
            _dcode = code;
            _centro = new TDoublePoint(cen.X, cen.Y);
        }

        /// <summary>
        /// Crea y traslada el Oblongo a la posición indicada.
        /// </summary>
        override
        public TPolygon Flash()
        {
            double r;
            TDoublePoint P1, P2;
            if (_ancho > _altura)
            {
                r = _altura / 2.0F;
                P1 = new TDoublePoint(_centro.X - _ancho / 2.0F + r, _centro.Y);
                P2 = new TDoublePoint(_centro.X + _ancho / 2.0F - r, _centro.Y);
            }
            else
            {
                r = _ancho / 2.0F;
                P1 = new TDoublePoint(_centro.X, _centro.Y - _altura / 2.0F +r);
                P2 = new TDoublePoint(_centro.X, _centro.Y + _altura / 2.0F - r);
            }
            _poligono = (new AperturaCircular(0, r * 2.0F)).Trazo(P1, P2);
            return _poligono;
        }

        /// <summary>
        /// Crea y traslada el Oblongo a la posición indicada.
        /// </summary>
        override
        public TPolygon Flash(TDoublePoint cen)
        {
            _centro = cen;
            return Flash();
        }

    }

    /// <summary>
    /// Define la Apertura Rectangular usada por gerber 
    /// </summary>

	public class AperturaRectangular:Apertura
    {
        double _altura = 0.0F;
        double _ancho = 0.0F;
        Char _tipo = 'R';

        /// <summary>
        /// Obtiene el alto del rectangulo.
        /// </summary>
        public double Altura
        {
            get { return _altura; }
            set { _altura = value; }
        }

        /// <summary>
        /// Obtiene el ancho del rectángulo.
        /// </summary>
        public double Ancho
        {
            get { return _ancho; }
            set { _ancho = value; }
        }

        /// <summary>
        /// Crea un cuadrado con las dimensiones indicadas.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="alt"></param>
        public AperturaRectangular(int code, double anc)
		{
            _dcode = code;
            _ancho = anc;
            _altura = anc;
            _centro = new TDoublePoint(0, 0);

		}

        /// <summary>
        /// Crea un cuadrado con las dimensiones indicadas.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="alt"></param>
        /// <param name="cen"></param>
        public AperturaRectangular(int code, double anc, TDoublePoint cen)
        {
            _dcode = code;
            _ancho = anc;
            _altura = anc;
            _centro = new TDoublePoint(cen.X, cen.Y);

        }

        /// <summary>
        /// Crea un Rectangulo con las dimensiones indicadas ubicado en el origen.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="anc"></param>
        /// <param name="alt"></param>
        public AperturaRectangular(int code, double anc, double alt)
		{
            _dcode = code;
            _ancho = anc;
            _altura = alt;
            _centro = new TDoublePoint(0, 0);

		}

        /// <summary>
        /// CCrea una geometría rectangular con las dimensiones indicadas.
        /// </summary>
        /// <param name="code">Código de la apertura</param>
        /// <param name="anc">Ancho del rectángulo</param>
        /// <param name="alt">Alto del rectángulo</param>
        /// <param name="cen">Centro del rectángulo</param>
        public AperturaRectangular(int code, double anc, double alt, TDoublePoint cen)
        {
            _dcode = code;
            _ancho = anc;
            _altura = alt;
            _centro = new TDoublePoint(cen.X,cen.Y);
        }
        /// <summary>
        /// Crea un Rectangulo con las dimensiones, ubicado en la posicion indicada.
        /// </summary>
        override
        public TPolygon Flash()
        {
            _poligono = new TPolygon();
            _poligono.Add(new TDoublePoint(_centro.X + (_ancho / 2), _centro.Y + (_altura / 2)));
            _poligono.Add(new TDoublePoint(_centro.X - (_ancho / 2), _centro.Y + (_altura / 2)));
            _poligono.Add(new TDoublePoint(_centro.X - (_ancho / 2), _centro.Y - (_altura / 2)));
            _poligono.Add(new TDoublePoint(_centro.X + (_ancho / 2), _centro.Y - (_altura / 2)));
            _poligono.Add(new TDoublePoint(_centro.X + (_ancho / 2), _centro.Y + (_altura / 2)));
            return _poligono;
        }

        /// <summary>
        /// Crea un Rectangulo ubicado en la posicion indicada.
        /// </summary>
        /// <param name="cen"></param>
        /// <returns></returns>
        override
		public TPolygon Flash(TDoublePoint cen)
		{
            _centro = new TDoublePoint(cen.X, cen.Y);
            return Flash();
		}
    }
}
