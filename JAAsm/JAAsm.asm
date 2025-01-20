.code
LagrangeAsm proc
; Argumenty:
    ; RCX: liCoefficients
    ; RDX: x
    ; R8: newCoefficients
    ; R9: j (int)
    ; [rsp+40]: i (int)
    ; [rsp+48]: degree (int)

    ;push rbx
    ;push rdi

    ; Przesuniêcie wspó³czynników i dodanie nowego sk³adnika
    mov r11, [rsp+48]
    shl r11, 2
loop_dec:
    test r11, r11
    jz decrease ; if k > 0

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
    test r11, r11
    jge loop_dec

;Obliczanie denominatora
    mov r10, [rsp+40]
    movdqu xmm0, [rdx+4*r10] ;xmm0 = x[i]
    movdqu xmm1, [rdx+4*r9]
    subss xmm0, xmm1 ;xmm0 = x[i] - x[j]

    xor r11, r11
    mov r12, [rsp+48]
    shl r12, 2

loop_inc:
;newCoefficients[k] /= denominator;
    movdqu xmm1, [R8+r11]
    divss xmm1, xmm0
    movdqu [R8+r11], xmm1

    add r11, 4
    test r11, r12
    jle loop_inc

;liCoefficients = newCoefficients
;tutaj dorobic kopiowanie newCoefficients do liCoefficients lub zwracanie tablicy R8


    ;pop rdi
    ;pop rbx
    ret
LagrangeAsm endp
end