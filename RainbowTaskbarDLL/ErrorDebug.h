#pragma once

struct IErrorDebug
{
private:
    size_t line;
    char* file;
};


#define _E this->line = __LINE__;
#define _F this->file = (char*)__FILE__;

#define __EFMT(buf,X) swprintf_s(buf, L"Exception on line %d, in file %hs: %ls", this->line, this->file, X); MessageBoxW(0, buf, L"RainbowTaskbarDLL Error", 0);
#define _EFMT(buf,x) __EFMT(buf,x)