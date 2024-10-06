#pragma once
#include "winrt.h"
#include "VisualTreeWatch.h"
#include <combaseapi.h>
#include <ocidl.h>
#include <xamlOM.h>
#include "AppearanceServiceAPI.h"
#include "ErrorDebug.h"



class TAP : public winrt::implements<TAP, IObjectWithSite, IErrorDebug, winrt::non_agile>
{
private:
    size_t line = 0;
    char* file = 0;
public:
    HRESULT STDMETHODCALLTYPE SetSite(IUnknown* pUnkSite) override;
    HRESULT STDMETHODCALLTYPE GetSite(REFIID riid, void** ppvSite) noexcept override;

    winrt::com_ptr<IVisualTreeService3> visualTreeService;
    winrt::com_ptr<VisualTreeWatch> visualTreeWatch;
};

