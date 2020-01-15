using System.Drawing;
using Emgu.CV;
using Leap;
namespace ModuloReconocimientoGestual
{
    // Interface que establece una firma para los complementos de reconocimeinto gestual
    public interface IPlugin
    {
        // Nombre del plugin
        string Name { get; }
        // Punto detectado por el plugin donde señala el usuario 
        Point Center { get; }
        // Estado del plugin que informa si se esta detectando un gesto
        bool DetectGesture { get; }
        // propiedad del plugin que informa si el plugnin tiene la capacidad detectar un click
        bool AutoClick { get; }
        // Estado del plugin que informa si se detecto un click
        bool DetectClick { get; }
        // Propiedad del plugin que informa de si se puede realizar una captrura automatica de imagenes (Camara)
        bool AutoCamCapture { get; }
        // Metodo del plugin que ejecuta su proceso principal eldetectado por el plugin que permite capturar un gesto
        Mat RunPlugin(VideoCapture src);
        
    }
}
