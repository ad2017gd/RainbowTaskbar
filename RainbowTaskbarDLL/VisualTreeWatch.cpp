
#include "VisualTreeWatch.h"
#include "winrt.h"
#include "Taskbar.h"
#include <winrt/Windows.UI.Xaml.Hosting.h>
#include <windows.ui.xaml.hosting.desktopwindowxamlsource.h>
#include <winrt/Windows.UI.Xaml.h>
#include <winrt/Windows.UI.Xaml.Shapes.h>
#include <winrt/Windows.UI.Xaml.Media.h>
#include <winrt/Windows.UI.Core.h>

#include <comdef.h>
#include <tchar.h>
#include <inspectable.h>
#include <winstring.h>
#include <fstream>
#include <winrt/base.h>

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

std::vector<std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>> VisualTreeWatch::FindChildrenRecursive(std::optional<std::wstring> name, winrt::Windows::UI::Xaml::UIElement root, int deep = 0)
try {
    auto children = std::vector<std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>>();
  
    auto childrenc = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChildrenCount(root);
    for (int i = 0; i < childrenc; i++) {
        auto child_ = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChild(root, i);
        auto child = child_.try_as< winrt::Windows::UI::Xaml::FrameworkElement>();
        if (!name.has_value() || (child && child.Name() == name.value())) {
            children.push_back(std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>(deep, child));
        }
        auto recurse = FindChildrenRecursive(name, child, deep+1);
        children.insert(children.end(), recurse.begin(), recurse.end());
        
  
    }
    return children;
}
catch (...) {
    return std::vector<std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>>();
}

winrt::Windows::UI::Xaml::FrameworkElement VisualTreeWatch::FindChildRecursive(std::wstring name, winrt::Windows::UI::Xaml::UIElement root)
try {
    auto childrenc = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChildrenCount(root);
    for (int i = 0; i < childrenc; i++) {
        auto child_ = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChild(root, i);
        auto child = child_.try_as< winrt::Windows::UI::Xaml::FrameworkElement>();
        if (child && child.Name() == name) {
            return child;
        }
        auto recurse = FindChildRecursive(name, child);
        if (recurse) return recurse;


    }
    return nullptr;
}
catch (...) {
    return nullptr;
}

winrt::Windows::Foundation::IAsyncAction VisualTreeWatch::StartYPosTask(Taskbar* taskbar) try { _F
_E    data->lTaskbarInfo[taskbar->tInfoIndex]._reserved[0] = 1;
_E    co_await winrt::resume_background();
_E    co_await winrt::resume_after(std::chrono::seconds(1));
      while (true) {
_E        co_await winrt::resume_foreground(dispatcher);
_E        auto v = taskbar->gripper.TransformToVisual(root).TransformPoint(winrt::Windows::Foundation::Point(0, 0)).Y;
_E        co_await winrt::resume_background();
_E        data->lTaskbarInfo[taskbar->tInfoIndex].YPosition = v;
_E        co_await winrt::resume_after(std::chrono::milliseconds(16));
      }
}
catch (...) {
    HRESULT res = winrt::to_hresult();
    _com_error err(res);
    WCHAR data[1024];
    __EFMT(data, err.ErrorMessage());
}
#include <cwchar>
winrt::Windows::Foundation::IAsyncAction VisualTreeWatch::StartUITask(Taskbar* taskbar) try { _F
_E    data->lTaskbarInfo2[taskbar->tInfoIndex].UIData = malloc(1024 * sizeof(wchar_t));
_E    memset(data->lTaskbarInfo2[taskbar->tInfoIndex].UIData, 0, 1024 * sizeof(wchar_t));
_E
_E    data->lTaskbarInfo2[taskbar->tInfoIndex].UIDataSize = 1024 * sizeof(wchar_t);
_E
_E    co_await winrt::resume_background();
_E    co_await winrt::resume_after(std::chrono::seconds(1));
_E    co_await winrt::resume_foreground(dispatcher);
_E
_E    auto start = taskbar->frame.Parent().try_as< winrt::Windows::UI::Xaml::FrameworkElement>();
_E
_E    auto SystemTrayFrame = FindChildRecursive(L"SystemTrayFrameGrid", start);
_E    auto TaskbarFrameRepeater = FindChildRecursive(L"TaskbarFrameRepeater", start);
_E    auto StartButton = FindChildRecursive(L"LaunchListButton", TaskbarFrameRepeater);
_E
_E    auto stfPt = winrt::Windows::Foundation::Point(0, 0);
_E    auto stfSize = winrt::Windows::Foundation::Size(0, 0);
_E    auto tfrPt = winrt::Windows::Foundation::Point(0, 0);
_E    auto tfrSize = winrt::Windows::Foundation::Size(0, 0);
_E
_E    co_await winrt::resume_background();
_E    
      while (true) {

_E        co_await winrt::resume_foreground(dispatcher);

_E        auto n_stfPt = SystemTrayFrame.TransformToVisual(root).TransformPoint(winrt::Windows::Foundation::Point(0, 0));
_E        auto n_stfSize = SystemTrayFrame.DesiredSize();
_E        auto n_tfrPt = StartButton.TransformToVisual(root).TransformPoint(winrt::Windows::Foundation::Point(0, 0));
_E        auto n_tfrSize = TaskbarFrameRepeater.DesiredSize();
_E
_E        co_await winrt::resume_background();
_E
_E        if (n_stfPt != stfPt || n_stfSize != stfSize || n_tfrPt != tfrPt || n_tfrSize != tfrSize) {
_E          std::swprintf((wchar_t*)data->lTaskbarInfo2[taskbar->tInfoIndex].UIData, data->lTaskbarInfo2[taskbar->tInfoIndex].UIDataSize - 1,
                L"{\"stfPt\":[%f,%f],\"stfSize\":[%f,%f],\"tfrPt\":[%f,%f],\"tfrSize\":[%f,%f]}", n_stfPt.X, n_stfPt.Y, n_stfSize.Width, n_stfSize.Height, n_tfrPt.X, n_tfrPt.Y, n_tfrSize.Width, n_tfrSize.Height);
_E            stfPt = n_stfPt;
_E            stfSize = n_stfSize;
_E            tfrPt = n_tfrPt;
_E            tfrSize = n_tfrSize;
_E        }
_E
_E        co_await winrt::resume_after(std::chrono::milliseconds(16));
      }
}
catch (...) {
    HRESULT res = winrt::to_hresult();
    _com_error err(res);
    WCHAR data[1024];
    __EFMT(data, err.ErrorMessage());
}

HRESULT VisualTreeWatch::OnVisualTreeChange(ParentChildRelation rel, VisualElement element, VisualMutationType mutationType)
{_F
    try {
        // Navigate down the visual tree and find the taskbarframes
    // * this is required because we cannot get the DesktopWindowXamlSource by navigating up the tree

    // we also cannot get its children so we wait for an element that has it as its parent, 
    // its weird idk
_E      auto changed = ConvertFromH< winrt::Windows::UI::Xaml::Hosting::IDesktopWindowXamlSource>(rel.Parent);
_E      if (changed && mutationType == Add) {
_E          root = ConvertFromH<winrt::Windows::UI::Xaml::FrameworkElement>(element.Handle);
_E          if (root) {
_E
_E              auto frame = FindChildRecursive(L"TaskbarFrame", root);
_E              if(frame){
                    [&]() -> winrt::Windows::Foundation::IAsyncAction {
                        if (dispatcher) {
                            co_await winrt::resume_background();
                            co_await winrt::resume_after(std::chrono::milliseconds(1000));
                            co_await winrt::resume_foreground(dispatcher);
                        }
_E                      InstanceHandle handle = ConvertToH(frame);
_E
_E                      auto rect_ = FindChildRecursive(L"BackgroundFill", frame);
_E                      if(rect_){
_E                          auto rect = rect_.try_as<winrt::Windows::UI::Xaml::Shapes::Rectangle>();
_E                          if (rect) {
_E                              
_E                              if (taskbarMap.find(handle) == taskbarMap.end()) {
_E                                  Taskbar t = Taskbar();
_E                                  t.originalBrush = rect.Fill();
_E                                  t.rectangleBackground = rect;
_E                                  t.dispatcher = rect.Dispatcher();
_E                                  t.frame = frame;
_E                                  HWND hwnd;
_E                                  changed.try_as<IDesktopWindowXamlSourceNative>()->get_WindowHandle(&hwnd);
_E                                  t.handle = GetParent(hwnd);
_E                                  for (int i = 0; i < 8; i++) {
_E                                      if (!data->lTaskbarInfo[i].taskbar) {
_E                                          data->lTaskbarInfo[i].taskbar = t.handle;
_E                                          data->lTaskbarInfo2[i].taskbar = t.handle;
_E                                          t.tInfoIndex = i;
_E                                          break;
_E                                      }
_E                                  }

_E                                  
_E
_E
_E                                  taskbarMap.emplace(handle, t);
_E                              }
_E                          }
_E                      }
_E
_E                      auto border_ = FindChildRecursive(L"BackgroundStroke", frame);
_E                      if(border_){
_E                          auto border = border_.try_as<winrt::Windows::UI::Xaml::Shapes::Rectangle>();
_E                          if (border) {
_E                              if(taskbarMap.find(handle) != taskbarMap.end()) 
                                    taskbarMap.find(handle)->second.border = border;
_E                          }
_E                      }
                        
_E
_E                      auto gripper = FindChildRecursive(L"GripperControl", frame);
_E                      if (gripper) {
_E                          if(taskbarMap.find(handle) != taskbarMap.end()) {
_E                              auto& tb = taskbarMap.find(handle)->second;
_E                              tb.gripper = gripper.try_as< winrt::Windows::UI::Xaml::FrameworkElement>();
_E                              StartYPosTask(&taskbarMap.find(handle)->second);
_E                              
_E                          
_E                          }
_E                      }
                        StartUITask(&taskbarMap.find(handle)->second);
                    }();
_E
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