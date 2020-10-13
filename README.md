В этом проекте реализован алгоритм планирования пути для car-like робота который едет через лабиринт.
Баранов Владимир (vsbaranov83@gmail.com)
для Unity 2019.3.11f1

Алгоритм планирования пути RRT. 
(Входит в состав Unity-проекта car_model.)

Алгоритмическая задача: исследовать алгоритм RRT для управления чтобы обходить коллизии и проходить маршрут от начальной точки, через промежуточные и до конечной
Алгоритм Rapidly-exploring Random Trees. (см. статью PlanningforDynamicVeh-1.pdf)

Програмный модуль rrt_planer.cs
На первом этапе можно реализовать алгоритм простого дерева без интегрирования траектории по Рунге-Кутта (как описано в LavKuf01.pdf).  Figure 2.
В качестве множества препятствий можно использовать что-то похожее на этот рисунок.
В юнити это означает что нужно вместо линий нарисовать протяженные объекты типа параллепипедов. 
Переход к лабиринту осуществится на поздних итерациях проекта.
На первых этапах сосредоточится на построении графа и отрисовке траектории в виде линий и не думать пока о объекте управления. Потом подключить кинематическую модель машины для расчета поворотов. Построить траектории в виде кривых. Когда будет работать алгоритм планирования пути можно будет подключать скрипты управления машиной . В качестве входа взять граф управления построенный по алгоритму RRT (rrt_planer.cs)

Вопросы по алгоритмической части: 
Как выбирать распределение в функции random_config.(подход goal biased или uniform)?
В функции new_state что выбирать за u скорость или угол поворота колес, и то и другое?
Из какого множества выбирать скорости или углы при каждом вызове new_state?
По какому критерию выбирать одну конкретную траекторию из множества всех возможных в графе?

Что мне не нравится в этой картинке?
Во первых траектория меняется слишком резко. Это предположительно изза того что функция steering имеет случайный характер (см. следующий заголовок)
Bторое есть дефекты в функции детектора коллизий.
Нам нужна функция сглаженного галсового шума с кубической сплайновой интерполяцией.

Тест распределения.
    1. Подключить TestDll к скрипту RttPlaner.cs и сделать один делегат GenerateCoordinates
    2. Подготовить тест в RttPlaner.cs тест будет отправлять в TestDll параметры распределения максимальные и минимальные значения для X и Z компоненты. На выходе c++ код сгенерирует два массива типа double с координатами
    3. Чтобы проверить правильность генерации координат можно создать сферы точечного размера в координатах из этих двух массивов