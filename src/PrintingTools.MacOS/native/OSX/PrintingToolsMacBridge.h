#pragma once

#ifdef __cplusplus
extern "C" {
#endif

void* PrintingTools_CreatePrintOperation(void* context);
void  PrintingTools_DisposePrintOperation(void* operation);
void  PrintingTools_BeginPreview(void* operation);
void  PrintingTools_CommitPrint(void* operation);

#ifdef __cplusplus
}
#endif
