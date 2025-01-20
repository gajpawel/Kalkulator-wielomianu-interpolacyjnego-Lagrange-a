.code
LagrangeAsm proc
; Argumenty:
    ; RCX: liCoefficients
    ; RDX: x
    ; R8: newCoefficients
    ; R9: j (int)
    ; [rsp+40]: i/l (int)
    ; [rsp+48]: degree (int)

    mov r10, [rsp+40]
    mov rax, [rsp+48]

    push rbx
    push rbp
    push rdi
    push rsi
    push r12
    push r13
    push r14
    push r15

    ; Przesuniêcie wspó³czynników i dodanie nowego sk³adnika
    mov r11, rax
    shl r11, 2
loop_dec:
    cmp r11, 0
    jle decrease ; if k > 0

;newCoefficients[k] += liCoefficients[k - 1]
    movdqu xmm2, [R8+r11]
    movdqu xmm3, [RCX+r11-4]
    addss xmm2, xmm3
    movdqu [R8+r11], xmm2
    
decrease:
;newCoefficients[k] -= liCoefficients[k] * x[j]
    movdqu xmm2, [RCX+r11]
    movdqu xmm3, [RDX+4*R9]
    mulss xmm2, xmm3
    movdqu xmm3, [R8+r11]
    subss xmm3, xmm2
    movdqu [R8+r11], xmm3

    sub r11, 4
    cmp r11, 0
    jge loop_dec

;Obliczanie denominatora
    movdqu xmm0, [rdx+4*r10] ;xmm0 = x[i]
    movdqu xmm1, [rdx+4*r9]
    subss xmm0, xmm1 ;xmm0 = x[i] - x[j]

    xor r11, r11
    mov r12, rax
    shl r12, 2

loop_inc:
;newCoefficients[k] /= denominator;
    movdqu xmm1, [R8+r11]
    divss xmm1, xmm0
    movdqu [R8+r11], xmm1

    add r11, 4
    cmp r11, r12
    jle loop_inc

    pop r15
    pop r14
    pop r13
    pop r12
    pop rsi
    pop rdi
    pop rbp
    pop rbx
    ret
LagrangeAsm endp
end