.code
LagrangeAsm proc
; Argumenty:
    ; RCX: liCoefficients
    ; RDX: x
    ; R8: newCoefficients
    ; R9: j (int)
    ; [rsp+20h]: i (int)
    ; [rsp+28h]: degree (int)

    push rbx
    push rdi

    ; Przesuniêcie wspó³czynników i dodanie nowego sk³adnika
    mov r11, [rsp+28h]
loop_dec:
    test r11, r11
    jz decrease ; if k > 0

;newCoefficients[k] += liCoefficients[k - 1]
    movdqu xmm2, [R8+r11*4]
    movdqu xmm3, [RCX+r11*4-4]
    addss xmm2, xmm3
    movdqu [R8+r11*4], xmm2
    
decrease:
;newCoefficients[k] -= liCoefficients[k] * x[j]
    movdqu xmm2, [RCX+r11*4]
    movdqu xmm3, [RDX+R9*4]
    mulss xmm2, xmm3
    movdqu xmm3, [R8+r11*4]
    subss xmm3, xmm2
    movdqu [R8+r11*4], xmm3

    sub r11, 1
    test r11, r11
    jg loop_dec

;Obliczanie denominatora
    mov r10, [rsp+20h]
    movdqu xmm0, [rdx+r10*4] ;xmm0 = x[i]
    movdqu xmm1, [rdx+r9*4]
    subss xmm0, xmm1 ;xmm0 = x[i] - x[j]

    xor r11, r11

loop_inc:
;newCoefficients[k] /= denominator;
    movdqu xmm1, [R8+r11*4]
    divss xmm1, xmm0
    movdqu [R8+r11*4], xmm1

    add r11, 1
    test r11, [rsp+28h]
    jle loop_inc

;liCoefficients = newCoefficients
;tutaj dorobic kopiowanie newCoefficients do liCoefficients lub zwracanie tablicy R8


    pop rdi
    pop rbx
    ret
LagrangeAsm endp
end