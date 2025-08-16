#include "AppearanceServiceAPI.h"
#include "Taskbar.h"
#include "winrt.h"
#include <comdef.h>
#include <tchar.h>

#include <winrt/Windows.UI.Core.h>
HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::SetAppearanceType(UINT type) {_F
    if (!active) return S_FALSE;
    try {
_E      auto watch = treeWatch.get();
_E      for (auto& [handle, taskbar] : watch->taskbarMap) {
_E          Taskbar& t = taskbar;//?
_E          switch (type) {
_E          case 0: // default
_E              taskbar.dispatcher.RunAsync(winrt::Windows::UI::Core::CoreDispatcherPriority::High, [&]() {
_E                  t.rectangleBackground.Fill(t.originalBrush);
_E                  t.border.Opacity(1);
_E              });
_E              
_E              break;
_E          case 2: // transparent
_E              taskbar.dispatcher.RunAsync(winrt::Windows::UI::Core::CoreDispatcherPriority::High, [&]() {
_E                  t.rectangleBackground.Fill(winrt::Windows::UI::Xaml::Media::SolidColorBrush(winrt::Windows::UI::Colors::Transparent()));
_E                  t.border.Opacity(0);
_E              });
_E              
_E              break;
_E
_E          case 1: // blurred (?)
_E              taskbar.dispatcher.RunAsync(winrt::Windows::UI::Core::CoreDispatcherPriority::High, [&]() {
_E                  auto brush = winrt::Windows::UI::Xaml::Media::AcrylicBrush();
_E                  brush.TintColor(winrt::Windows::UI::Colors::Transparent());
_E                  brush.TintOpacity(0);
_E                  brush.TintLuminosityOpacity(0.0);
_E                  brush.BackgroundSource(winrt::Windows::UI::Xaml::Media::AcrylicBackgroundSource::Backdrop);
_E                  t.rectangleBackground.Fill(brush);
_E                  t.border.Opacity(1);
_E              });
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


HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::Close() try { _F
_E  SetAppearanceType(0);
_E  return 0;
}
catch (...)
{
    HRESULT res = winrt::to_hresult();
    _com_error err(res);
    WCHAR data[1024];
    __EFMT(data, err.ErrorMessage());
    return res;
}

HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::Version() { 

    return 4;
}

//#include <fstream>
//HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::DebugGetUITree(BSTR* tree) try { _F
//_E  auto watch = treeWatch.get();
//
//_E  if (!watch->taskbarMap.begin()->first) return S_FALSE;
//_E  std::wstring str = std::wstring();
//    for (auto& taskbar : watch->taskbarMap) {
//_E      auto frame = watch->ConvertFromH<winrt::Windows::UI::Xaml::FrameworkElement>(taskbar.first);
//_E      auto root = frame.Parent().as<winrt::Windows::UI::Xaml::FrameworkElement>();
//_E      auto children = watch->FindChildrenRecursive(std::nullopt, root, 0);
////_E      std::sort(children.begin(), children.end(), [&](std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>& a, std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>& b) { return a.first < b.first; });
//_E      for (auto& child : children) {
//_E          auto iinsp = watch->ConvertFromH<IInspectable>(watch->ConvertToH(child.second));
//_E          winrt::hstring str3 = winrt::get_class_name(iinsp);
//_E          auto add = std::wstring(child.first*2, ' ');
//_E          auto pt = child.second.TransformToVisual(watch->root).TransformPoint(winrt::Windows::Foundation::Point(0, 0));
//_E          auto felem = child.second.try_as< winrt::Windows::UI::Xaml::FrameworkElement>();
//_E          str = str + add + std::wstring{ str3 } + L" " + (felem ? std::wstring{ felem.Name() } : L"NULL") + L" " + L" " + std::to_wstring(pt.X) + L" " + std::to_wstring(pt.Y) + L"\n";
//
//_E
//        }
//        str = str + L"\n\n\n";
//    }
//_E  BSTR bstr = SysAllocStringLen(str.data(), str.size());
//_E  if (tree)
//        *tree = bstr;
//
//} catch (...) {
//    HRESULT res = winrt::to_hresult();
//    
//    _com_error err(res);
//    WCHAR data[1024];
//    __EFMT(data, err.ErrorMessage());
//    return res;
//}

\

HRESULT STDMETHODCALLTYPE AppearanceServiceAPI::GetDataPtr(){
    auto watch = treeWatch.get();
    return (HRESULT)watch->data;
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
    {
        HRESULT hr = Close();
        if (FAILED(hr))
            return hr;
        return S_OK;
    }
    case 2:
        return Version();
    case 3: {
        return GetDataPtr();
    }

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

void AppearanceServiceAPI::Unload() {
    CoRevokeClassObject(s_proxyCookie);
}

DWORD s_proxyCookie = 0;
DWORD s_activeObjectCookie = 0;

AppearanceServiceAPI::AppearanceServiceAPI(winrt::com_ptr<VisualTreeWatch> watch) try {_F
    treeWatch = watch;

    watch->data = (struct RainbowTaskbarData*)malloc(sizeof(struct RainbowTaskbarData));
    memset(watch->data, 0, sizeof(struct RainbowTaskbarData));

    winrt::com_ptr<IUnknown> stub;
    const CLSID CLSID_PROXY = PROXY_CLSID_IS;
_E  winrt::check_hresult(DllGetClassObject(CLSID_PROXY, winrt::guid_of<decltype(stub)::type>(), (void**)stub.put()));
_E  winrt::check_hresult(CoRegisterClassObject(CLSID_PROXY, stub.get(), CLSCTX_INPROC_SERVER, REGCLS_MULTIPLEUSE, &s_proxyCookie));
_E
_E  winrt::check_hresult(RegisterActiveObject(static_cast<IDispatch*>(this), CLSID_AppearanceServiceAPI, ACTIVEOBJECT_STRONG, &s_activeObjectCookie));

}
catch (...)
{
    HRESULT res = winrt::to_hresult();
    _com_error err(res);
    WCHAR data[1024];
    __EFMT(data, err.ErrorMessage());
}