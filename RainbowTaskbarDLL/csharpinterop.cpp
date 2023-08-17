#include "winrt.h"
#include "RainbowTaskbarDLL_h.h"

__declspec(dllexport) STDAPI SetAppearanceTypeDLL(UINT type) {
    HRESULT hr;
    winrt::com_ptr<IUnknown> _API;

    if ((hr = GetActiveObject(CLSID_AppearanceServiceAPI, 0, _API.put())) != 0) {
        return hr;
    }

    winrt::com_ptr<IDispatch> API = _API.try_as<IDispatch>();
    if (API == nullptr) {
        return E_FAIL; // lol
    }
    else {
        VARIANTARG arg;
        arg.vt = VT_UI4;
        arg.ulVal = type;

        DISPPARAMS params;
        params.cArgs = 1;
        params.cNamedArgs = 0;
        params.rgvarg = &arg;
        hr = API->Invoke(0, IID_NULL, 0, DISPATCH_METHOD, &params, 0, 0, 0);
        return hr;
    }

}

__declspec(dllexport) STDAPI CloseDLL() {
    HRESULT hr;
    winrt::com_ptr<IUnknown> _API;

    if (FAILED(hr = GetActiveObject(CLSID_AppearanceServiceAPI, 0, _API.put()))) {
        return hr;
    }

    winrt::com_ptr<IDispatch> API = _API.try_as<IDispatch>();
    if (API == nullptr) {
        return E_FAIL; // lol
    }
    else {
        DISPPARAMS params;
        params.cArgs = 0;
        params.cNamedArgs = 0;
        hr = API->Invoke(1, IID_NULL, 0, DISPATCH_METHOD, &params, 0, 0, 0);
        return hr;
    }
}