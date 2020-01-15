using System.Collections.Generic;
using DirectShowLib;

namespace ModuloProcesamientoImagenes
{   //Clase que obtiene datos y configura las camaras
    public class CameraActivity
    {
        List<KeyValuePair<int, string>> ListCamerasData;
        DsDevice[] _SystemCamereas;
        int _DeviceIndex;

        public CameraActivity()
        {

            ListCamerasData = new List<KeyValuePair<int, string>>();
            _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            _DeviceIndex = 0;

        }

        public List<KeyValuePair<int, string>> ListCameras()
        {
            foreach (DsDevice _Camera in _SystemCamereas)
            {

                ListCamerasData.Add(new KeyValuePair<int, string>(_DeviceIndex, _Camera.Name));
                _DeviceIndex++;
            }

            return ListCamerasData;
        }
    }
}
