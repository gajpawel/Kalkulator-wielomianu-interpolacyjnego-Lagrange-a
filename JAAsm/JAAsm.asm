.code
LagrangeAsm proc
    ; RCX = wska�nik do tablicy x
    ; RDX = wska�nik do tablicy y
    ; R8  = d�ugo�� tablicy
    ; R9  = wska�nik do tablicy wynikowej

    ; R11 b�dzie pe�ni� rol� indeksu w p�tli
    xor r11, r11          ; R11 = 0 (indeks startowy)

LoopStart:
    cmp r11, R8           ; Czy R11 < d�ugo�� tablicy?
    jge LoopEnd           ; Je�li nie, wyjd� z p�tli

    ; Za�aduj x[i] do XMM0
    movsd xmm0, qword ptr [RCX + r11 * 8]

    ; Za�aduj y[i] do XMM1
    movsd xmm1, qword ptr [RDX + r11 * 8]

    ; Dodaj x[i] + y[i] i zapisz wynik w XMM0
    addsd xmm0, xmm1

    ; Zapisz wynik do tablicy wynikowej
    movsd qword ptr [R9 + r11 * 8], xmm0

    ; Zwi�ksz indeks (R11++)
    inc r11
    jmp LoopStart          ; Powr�t do pocz�tku p�tli

LoopEnd:
    ret                    ; Zako�cz funkcj�
LagrangeAsm endp
end
