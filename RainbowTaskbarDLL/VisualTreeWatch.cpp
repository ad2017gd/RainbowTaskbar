
#include "VisualTreeWatch.h"
#include "winrt.h"
#include <winrt/Windows.UI.Xaml.Hosting.h>
#include <windows.ui.xaml.hosting.desktopwindowxamlsource.h>
#include <winrt/Windows.UI.Xaml.h>
#include <winrt/Windows.UI.Xaml.Shapes.h>
#include <winrt/Windows.UI.Xaml.Media.h>



winrt::Windows::UI::Xaml::FrameworkElement FindRootElement(winrt::Windows::UI::Xaml::FrameworkElement child)
{
    const auto parent = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetParent(child).try_as<winrt::Windows::UI::Xaml::FrameworkElement>();
    if (parent)
    {
        return FindRootElement(parent);
    }
    else
    {
        return child;
    }
    
}

std::vector<winrt::Windows::UI::Xaml::FrameworkElement> FindChildrenRecursive(std::optional<std::wstring> name, winrt::Windows::UI::Xaml::FrameworkElement root)
{

    auto children = std::vector<winrt::Windows::UI::Xaml::FrameworkElement>();

    auto childrenc = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChildrenCount(root);
    for (int i = 0; i < childrenc; i++) {
        auto child_ = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChild(root, i);
        auto child = child_.try_as< winrt::Windows::UI::Xaml::FrameworkElement>();
        if (!name.has_value() || child.Name() == name.value()) {
            children.push_back(child);
        }
        auto recurse = FindChildrenRecursive(name, child);
        children.insert(children.end(), recurse.begin(), recurse.end());

    }
    return children;
}

std::vector<winrt::Windows::UI::Xaml::FrameworkElement> FindAllChildren(winrt::Windows::UI::Xaml::FrameworkElement root)
{
    return FindChildrenRecursive(std::nullopt, root);
}

#include <comdef.h>
#include <tchar.h>
#include <inspectable.h>
#include <winstring.h>
#include <winrt/Windows.UI.Core.h>
HRESULT VisualTreeWatch::OnVisualTreeChange(ParentChildRelation rel, VisualElement element, VisualMutationType mutationType)
{_F
    try {
        // Navigate down the visual tree and find the taskbarframes

_E      auto changed = ConvertFromH< winrt::Windows::UI::Xaml::FrameworkElement>(element.Handle);
_E
_E      
_E      if (changed) {
_E          root = FindRootElement(changed);
_E          if (root) {
_E
_E              auto children = FindChildrenRecursive(L"TaskbarFrame", root);
_E              if (taskbarFrames < children.size() || changed.Name() == L"TaskbarFrame") {
_E                  taskbarFrames = children.size();
_E                  for (auto& frame : children) {
_E                      auto rects = FindChildrenRecursive(L"BackgroundFill", frame);
_E                      for (auto& rect_ : rects) {
_E                          auto rect = rect_.try_as<winrt::Windows::UI::Xaml::Shapes::Rectangle>();
_E                          if (rect) {
_E                              InstanceHandle handle = ConvertToH(frame);
_E
_E                              if (mutationType == Add) {
_E                                  if (taskbarMap.find(handle) == taskbarMap.end()) {
_E                                      Taskbar t = Taskbar();
_E                                      t.originalBrush = rect.Fill();
_E                                      t.rectangleBackground = rect;
_E                                      t.dispatcher = rect.Dispatcher();
_E                                      taskbarMap.emplace(handle, t);
_E                                  }
_E                              }
                                else {
_E                                  if (taskbarMap.find(handle) != taskbarMap.end()) {
_E                                      taskbarMap.erase(handle);
_E                                  }
_E                              }
_E
_E
_E                          }
_E                      }
_E
_E                  }
_E              }
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

void VisualTreeWatch::Stop() {
    xamlDiagnostics.as<IVisualTreeService3>()->UnadviseVisualTreeChange(this);
}


HRESULT VisualTreeWatch::OnElementStateChanged(InstanceHandle, VisualElementState, LPCWSTR) noexcept
{
    return S_OK;
}


VisualTreeWatch::VisualTreeWatch() {
    appService = winrt::make_self<AppearanceServiceAPI>(get_strong());
}