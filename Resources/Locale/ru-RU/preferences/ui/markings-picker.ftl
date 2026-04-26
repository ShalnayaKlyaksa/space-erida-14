markings-used = Используемые черты
markings-unused = Неиспользуемые черты
markings-add = Добавить черту
markings-remove = Убрать черту
markings-rank-up = Вверх
markings-rank-down = Вниз
marking-points-remaining = Черт осталось: { $points }
marking-used = { $marking-name }
marking-used-forced = { $marking-name } (Принудительно)
marking-slot-add = Добавить
marking-slot-remove = Удалить
marking-slot = Слот { $number }

humanoid-marking-modifier-force = Принудительно
humanoid-marking-modifier-ignore-species = Игнорировать рассу
humanoid-marking-modifier-base-layers = Базовый слой
humanoid-marking-modifier-prototype-id = ID прототипа:

# Categories

markings-category-Special = Специальное
markings-category-Hair = Причёска
markings-category-FacialHair = Лицевая растительность
markings-category-Head = Голова
markings-category-HeadTop = Голова (верх)
markings-category-HeadSide = Голова (бок)
markings-category-Snout = Морда
markings-category-SnoutCover = Морда (Поверх)
markings-category-UndergarmentTop = Нижнее бельё (Верх)
markings-category-UndergarmentBottom = Нижнее бельё (Низ)
markings-category-Chest = Грудь
markings-category-Arms = Руки
markings-category-Legs = Ноги
markings-category-Tail = Хвост
markings-category-Overlay = Наложение

markings-search = Поиск

-markings-selection = { $selectable ->
    [0] У вас не осталось доступных черт.
    [one] Вы можете выбрать ещё одну черту.
   *[other] Вы можете выбрать ещё { $selectable } черт.
}

markings-limits = { $required ->
    [true] { $count ->
        [-1] Выберите хотя бы одну черту.
        [0] Вы не можете выбрать ни одной черты, но каким-то образом обязаны? Это ошибка.
        [one] Выберите одну черту.
       *[other] Выберите как минимум одну и до {$count} черт. { -markings-selection(selectable: $selectable) }
    }
   *[false] { $count ->
        [-1] Вы можете выбрать любое количество черт.
        [0] Вы не можете выбрать ни одной черты.
        [one] Вы можете выбрать не более одной черты.
       *[other] Вы можете выбрать до {$count} черт. { -markings-selection(selectable: $selectable) }
    }
}

markings-reorder = Изменить порядок черт
markings-gradient = Градиент
markings-gradient-color = Цвет перехода
markings-gradient-angle = Направление
markings-gradient-offset = Смещение
markings-gradient-softness = Мягкость
markings-gradient-None = Нет
markings-gradient-Vertical = Вертикальный
markings-gradient-Horizontal = Горизонтальный
markings-gradient-Diagonal = Диагональный
markings-gradient-ReverseDiagonal = Обратная диагональ
markings-gradient-Radial = Радиальный

humanoid-marking-modifier-respect-limits = Соблюдать ограничения
humanoid-marking-modifier-respect-group-sex = Учитывать ограничения по группе и полу
humanoid-marking-modifier-enable = Включить

# Categories

markings-organ-Torso = Торс
markings-organ-Head = Голова
markings-organ-ArmLeft = Левая рука
markings-organ-ArmRight = Правая рука
markings-organ-HandRight = Правая кисть
markings-organ-HandLeft = Левая кисть
markings-organ-LegLeft = Левая нога
markings-organ-LegRight = Правая нога
markings-organ-FootLeft = Левая ступня
markings-organ-FootRight = Правая ступня
markings-organ-Eyes = Глаза

markings-layer-Special = Специальное
markings-layer-Tail = Хвост
markings-layer-Tail-Moth = Крылья
markings-layer-Hair = Причёска
markings-layer-FacialHair = Лицевая растительность
markings-layer-UndergarmentTop = Нижнее бельё (верх)
markings-layer-UndergarmentBottom = Нижнее бельё (низ)
markings-layer-Chest = Грудь
markings-layer-Head = Голова
markings-layer-Snout = Морда
markings-layer-SnoutCover = Морда (покрытие)
markings-layer-HeadSide = Голова (бок)
markings-layer-HeadTop = Голова (верх)
markings-layer-Eyes = Глаза
markings-layer-RArm = Правая рука
markings-layer-LArm = Левая рука
markings-layer-RHand = Правая кисть
markings-layer-LHand = Левая кисть
markings-layer-RLeg = Правая нога
markings-layer-LLeg = Левая нога
markings-layer-RFoot = Правая ступня
markings-layer-LFoot = Левая ступня
markings-layer-Overlay = Наложение
markings-layer-TailOverlay = Наложение
