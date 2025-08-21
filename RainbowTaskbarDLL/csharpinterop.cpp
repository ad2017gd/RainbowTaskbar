#include "winrt.h"
#include "RainbowTaskbarDLL_h.h"
#include <comdef.h>
#include <windows.h>
#include <winuser.h>


#include <winrt/Windows.ApplicationModel.h>

#define IF_GET_API                                                              \
HRESULT hr;                                                                     \
winrt::com_ptr<::IUnknown> _API;                                                  \
if (FAILED(hr = GetActiveObject(CLSID_AppearanceServiceAPI, 0, _API.put()))) {  \
    return hr;                                                                  \
}                                                                               \
winrt::com_ptr<::IDispatch> API = _API.try_as<::IDispatch>();                       \
if (API != nullptr)

#define INVOKE(x) API->Invoke(x, IID_NULL, 0, DISPATCH_METHOD, &params, 0, 0, 0)

#define NO_ARGS(x)                                                              \
DISPPARAMS params;                                                              \
params.cArgs = 0;                                                               \
params.cNamedArgs = 0;                                                          \
hr = INVOKE(x);                                                                 \
return hr



#define SAType 0
#define Close 1
#define Version 2
#define GetDataPtr 3
#define SetTaskbarElementsOpacity 4

#define DGetUiTree 10

__declspec(dllexport) STDAPI SetAppearanceTypeDLL(UINT type) {
    IF_GET_API {
        VARIANTARG arg = {0};
        arg.vt = VT_UI4;
        arg.ulVal = type;

        DISPPARAMS params;
        params.cArgs = 1;
        params.cNamedArgs = 0;
        params.rgvarg = &arg;
        hr = INVOKE(SAType);
        return hr;
    } else {
        return E_FAIL;
    }

}

__declspec(dllexport) STDAPI CloseDLL() {
    IF_GET_API {
        NO_ARGS(Close);
    
    } else {
        return E_FAIL;
    }
}
__declspec(dllexport) STDAPI VersionDLL() {
    IF_GET_API {
        NO_ARGS(Version);
    } else {
        return 0;
    }
}
__declspec(dllexport) STDAPI GetDataPtrDLL() {
    IF_GET_API {
        NO_ARGS(GetDataPtr);
    } else {
        return 0;
    }
}

__declspec(dllexport) STDAPI DebugGetUITreeDLL(BSTR* tree) {
    return E_FAIL;

}


__declspec(dllexport) STDAPI SetTaskbarElementsOpacityDLL(UINT opac) {
    IF_GET_API{
        VARIANTARG arg = {0};
        arg.vt = VT_UI4;
        arg.ulVal = opac;

        DISPPARAMS params;
        params.cArgs = 1;
        params.cNamedArgs = 0;
        params.rgvarg = &arg;
        hr = INVOKE(SetTaskbarElementsOpacity);
        return hr;
    }
else {
    return E_FAIL;
}

}