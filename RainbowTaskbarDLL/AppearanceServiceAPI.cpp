#include "AppearanceServiceAPI.h"
#include "Taskbar.h"
#include "winrt.h"
#include <comdef.h>
#include <tchar.h>


HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::SetAppearanceType(UINT type) {_F

    try {
_E      auto watch = treeWatch.get();
_E      for (auto& [handle, taskbar] : watch->taskbarMap) {
_E          switch (type) {
_E          case 0: // default
_E              taskbar.rectangleBackground.Fill(taskbar.originalBrush);
_E              break;
_E          case 2: // transparent
_E              taskbar.rectangleBackground.Fill(winrt::Windows::UI::Xaml::Media::SolidColorBrush(winrt::Windows::UI::Colors::Transparent()));
_E              break;
_E
_E          case 1: // blurred (?)
_E              auto brush = winrt::Windows::UI::Xaml::Media::AcrylicBrush();
_E              brush.TintColor(winrt::Windows::UI::Colors::Transparent());
_E              brush.TintOpacity(0);
_E              brush.TintLuminosityOpacity(0.0);
_E              brush.BackgroundSource(winrt::Windows::UI::Xaml::Media::AcrylicBackgroundSource::HostBackdrop);
_E              taskbar.rectangleBackground.Fill(brush);
_E              break;
_E          }
_E      }
    }
    catch (...) {
        HRESULT res = winrt::to_hresult();
        _com_error err(res);
        WCHAR data[1024];
        __EFMT(data, err.ErrorMessage());
        return res;
    }

    return S_OK;
}


HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::Close() {

    SetAppearanceType(0);
    CoRevokeClassObject(proxyCookie);
    RevokeActiveObject(activeObjectCookie, 0);
    treeWatch->Stop();
    return S_OK;
}

HRESULT AppearanceServiceAPI::Invoke(DISPID dispIdMember, // 0 or 1
    REFIID riid, // null
    LCID lcid, // null
    WORD wFlags, // DISPATCH_METHOD
    DISPPARAMS* pDispParams,
    VARIANT* pVarResult,
    EXCEPINFO* pExcepInfo,
    unsigned int* puArgErr
) {
    // this is such a shitty implementation but I DONT CARE!
    switch (dispIdMember)
    {
    case 0:
        if (pDispParams->cArgs == 1 && pDispParams->rgvarg[0].vt == VT_UI4)
        {
            UINT appearanceType = pDispParams->rgvarg[0].ulVal;
            HRESULT hr = SetAppearanceType(appearanceType);
            if (FAILED(hr))
                return hr;
        }
        return S_OK;

    case 1:
        HRESULT hr = Close();
        if (FAILED(hr))
            return hr;
        return S_OK;
    }

    return DISP_E_MEMBERNOTFOUND;
}
HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::GetTypeInfoCount(UINT* pctinfo)
{
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo)
{
    return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId)
{
    return E_NOTIMPL;
}

AppearanceServiceAPI::AppearanceServiceAPI(winrt::com_ptr<VisualTreeWatch> watch) try {_F
    treeWatch = watch;

    winrt::com_ptr<IUnknown> stub;
    const CLSID CLSID_PROXY = PROXY_CLSID_IS;
_E  winrt::check_hresult(DllGetClassObject(CLSID_PROXY, winrt::guid_of<decltype(stub)::type>(), (void**)stub.put()));
_E  winrt::check_hresult(CoRegisterClassObject(CLSID_PROXY, stub.get(), CLSCTX_INPROC_SERVER, REGCLS_MULTIPLEUSE, &proxyCookie));
_E
_E  winrt::check_hresult(RegisterActiveObject(static_cast<IDispatch*>(this), CLSID_AppearanceServiceAPI, ACTIVEOBJECT_STRONG, &activeObjectCookie));

}
catch (...)
{
    HRESULT res = winrt::to_hresult();
    _com_error err(res);
    WCHAR data[1024];
    __EFMT(data, err.ErrorMessage());
}