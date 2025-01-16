.code
LagrangeAsm proc
; Argumenty:
    ; RCX: wskaünik na liCoefficients
    ; RDX: wskaünik na x
    ; R8: degree (int)
    ; R9: i (int)
    ; [rsp+28h]: j (int)

    push rbx
    push rdi

    mov rax, rdx          ; Skopiuj adres tablicy z RCX do RAX
    movdqu xmm0, [rax+4]      ; Za≥aduj zerowy element tablicy do XMM3

    pop rdi
    pop rbx
    ret
LagrangeAsm endp
end

    ; Wczytanie argumentÛw
    movdqu xmm0, [rcx]
    movdqu xmm1, [rdx]
    mov rdi, rcx                    ; wskaünik na liCoefficients
    mov rsi, rdx                    ; wskaünik na x
    mov ecx, r8d                    ; degree
    mov eax, r9d                    ; indeks i
    mov edx, dword ptr [rsp+28h]    ; indeks j

    ; Obliczenie denominator = x[i] - x[j]
    movss xmm0, dword ptr [rsi + rax*4] ; xmm0 = x[i]
    movss xmm1, dword ptr [rsi + rdx*4] ; xmm1 = x[j] tu jest zg≥aszany wyjπtek
    subss xmm0, xmm1                 ; xmm0 = x[i] - x[j]

    ; Iteracja przez wspÛ≥czynniki liCoefficients
    xor ebx, ebx                     ; k = 0
loopx:
    ; liCoefficients[k] -= liCoefficients[k] * x[j]
    movss xmm2, dword ptr [rdi + rbx*4] ; xmm2 = liCoefficients[k]
    mulss xmm2, xmm1                 ; xmm2 *= x[j]
    subss xmm2, dword ptr [rdi + rbx*4] ; liCoefficients[k] -= xmm2
    divss xmm2, xmm0                 ; xmm2 /= denominator
    movss dword ptr [rdi + rbx*4], xmm2 ; zapis wyniku

    inc ebx                          ; k++
    cmp ebx, ecx                     ; k <= degree
    jle loopx

    pop rdi
    pop rbx
    ret
LagrangeAsm endp
end
