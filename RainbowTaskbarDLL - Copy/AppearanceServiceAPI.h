#pragma once
#include "winrt.h"
#include "VisualTreeWatch.h"
#include <winrt/Windows.Foundation.h>
#include "RainbowTaskbarDLL_h.h"
#include "ErrorDebug.h"

class AppearanceServiceAPI : public winrt::implements<AppearanceServiceAPI, IDispatch, IErrorDebug, winrt::non_agile>
{
private:
    size_t line = 0;
    char* file = 0;
public:
    winrt::com_ptr<VisualTreeWatch> treeWatch;

    DWORD proxyCookie = 0;
    DWORD activeObjectCookie = 0;
    bool active = true;

    HRESULT STDMETHODCALLTYPE SetAppearanceType(UINT type);

    HRESULT STDMETHODCALLTYPE Close();

    HRESULT STDMETHODCALLTYPE Version();

    HRESULT STDMETHODCALLTYPE GetDataPtr();

    HRESULT STDMETHODCALLTYPE DebugGetUITree(BSTR* tree);

    HRESULT STDMETHODCALLTYPE Invoke(DISPID dispIdMember,
        REFIID riid,
        LCID lcid,
        WORD wFlags,
        DISPPARAMS* pDispParams,
        VARIANT* pVarResult,
        EXCEPINFO* pExcepInfo,
        unsigned int* puArgErr
    ) override;

    // BORING!
    HRESULT STDMETHODCALLTYPE GetTypeInfoCount(
        UINT* pctinfo) override;

    HRESULT STDMETHODCALLTYPE GetTypeInfo(
        UINT iTInfo,
        LCID lcid,
        ITypeInfo** ppTInfo) override;

    HRESULT STDMETHODCALLTYPE GetIDsOfNames(
        REFIID riid,
        LPOLESTR* rgszNames,
        UINT cNames,
        LCID lcid,
        DISPID* rgDispId) override;


    AppearanceServiceAPI(winrt::com_ptr<VisualTreeWatch> watch);

    
};

