#include "TAP.h"
#include "AppearanceServiceAPI.h"
#include "RainbowTaskbarDLL_h.h"


#include <comdef.h>
#include <tchar.h>
#include <winrt/Windows.UI.Core.h>



HRESULT TAP::SetSite(IUnknown* pUnk) try
{_F
_E  winrt::com_ptr<IUnknown> cast;
_E  cast.copy_from(pUnk);
_E
_E  visualTreeService = cast.as<IVisualTreeService3>();

_E  visualTreeWatch = winrt::make_self<VisualTreeWatch>();
_E  visualTreeWatch->xamlDiagnostics = cast.as<IXamlDiagnostics>();
_E  
_E  visualTreeService->AdviseVisualTreeChange(visualTreeWatch.get());
_E  winrt::com_ptr<::IInspectable> _disp;
_E  visualTreeWatch->xamlDiagnostics->GetDispatcher(_disp.put());
_E  visualTreeWatch->dispatcher = _disp.as<winrt::Windows::UI::Core::CoreDispatcher>();
_E  return S_OK;
}
catch (...)
{
    HRESULT res = winrt::to_hresult();
    _com_error err(res);
    WCHAR data[1024];
    __EFMT(data, err.ErrorMessage());
    return res;
}

HRESULT TAP::GetSite(REFIID riid, void** ppvSite) noexcept
{
    return visualTreeService.as(riid, ppvSite);
}