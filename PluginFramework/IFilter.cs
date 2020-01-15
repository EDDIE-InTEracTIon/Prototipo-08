using System.Drawing;
using Emgu.CV;

namespace ModuloReconocimientoGestual
{
    // Interface que establece una firma para los complementos de reconocimeinto gestual
    public interface IPlugin
    {

        Mat RunPlugin(VideoCapture src);
        string Name { get; }
        Point Center { get; }
        bool DetectGesture { get; }
    }
}
