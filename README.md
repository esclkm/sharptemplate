sharptemplate
=============

C# Шаблонизатор

Шаблонизатор создан по мотивам шаблонизатора CMS Cotonti

Основная цель его создания:
Cotonti довольно малопригоден для быстрого создания модулей со списком элементов, добавлением, правкой элементов. Многие вещи приходится делать долго и кропотно. Мои CDT довольно слабо ускоряли создание новых модулей. А довольно известная в кругах котонти биржа, ничто иное как перевоплощение разный вариантов страниц.
Знакомство с GRUD и Gii в Yii 1.x приблизило мое желание к реализации

Понимание того, что создание всего этого внутри кода сильно усложнит понимание кода, потребовала создание шаблонизатора. Я не нашел ни одного шаблонизатора для C#, поэтому решил написать его сам. Даю его вам попробовать и протестировать. Спасибо за критику. 
Синтаксис шаблонов довольно отличается от такового в Котонти. Это потребовалось для возможности моделирования шаблонов Cotonti
Синтаксис шаблонов:
=====================
    %%VARIABLE%% // Переменная
    [[ BEGIN: BLOCK_NAME ]]Текст блока[[ END: BLOCK_NAME ]] // Это логический блок.
    [[ IF: CONDITION ]] Текст [[ ENDIF ]] // Логические блоки. 
        // CONDITION Поддеживает одинарые выражения с =, !=, >, <. 
        // Например: A = B, B != A, A > B, !A, B.

Использование класса:
=====================
    xtemplate t = new xtemplate("ИМЯФАЙЛА");
    t.assign("VAR", "Задаем переменную");
    t.parse("MAIN.ROW"); // парсим дочерний блок
    t.parse("MAIN"); // Парсим родительский блок
    t.save("MAIN","MAIN"); // Сохраняем блок в файл



