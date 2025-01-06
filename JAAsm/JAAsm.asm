.code
LagrangeAsm proc
    ; RCX = wskaŸnik do tablicy x
    ; RDX = wskaŸnik do tablicy y
    ; R8  = d³ugoœæ tablicy
    ; R9  = wskaŸnik do tablicy wynikowej

    ; R11 bêdzie pe³ni³ rolê indeksu w pêtli
    xor r11, r11          ; R11 = 0 (indeks startowy)

LoopStart:
    cmp r11, R8           ; Czy R11 < d³ugoœæ tablicy?
    jge LoopEnd           ; Jeœli nie, wyjdŸ z pêtli

    ; Za³aduj x[i] do XMM0
    movsd xmm0, qword ptr [RCX + r11 * 8]

    ; Za³aduj y[i] do XMM1
    movsd xmm1, qword ptr [RDX + r11 * 8]

    ; Dodaj x[i] + y[i] i zapisz wynik w XMM0
    addsd xmm0, xmm1

    ; Zapisz wynik do tablicy wynikowej
    movsd qword ptr [R9 + r11 * 8], xmm0

    ; Zwiêksz indeks (R11++)
    inc r11
    jmp LoopStart          ; Powrót do pocz¹tku pêtli

LoopEnd:
    ret                    ; Zakoñcz funkcjê
LagrangeAsm endp
end
