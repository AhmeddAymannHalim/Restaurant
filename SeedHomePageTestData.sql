/* Test data for the public home page: gallery, kitchen team (chefs), menu sections + items.
   Idempotent: every insert is guarded, safe to re-run. Image paths are files already in wwwroot. */
SET NOCOUNT ON;
USE RestaurantDatabase;
BEGIN TRAN;

-- ---------- Gallery ----------
INSERT INTO GalleryItem (PictureUrl)
SELECT v.p FROM (VALUES
 ('/Images/gallery-1.jpg'),('/Images/gallery-2.jpg'),('/Images/gallery-3.jpg'),('/Images/gallery-4.jpg'),('/Images/gallery-5.jpg'),
 ('/Images/food.jpg'),('/Images/about.jpg'),('/Images/bread-barrel.jpg'),('/Images/burger.jpg'),('/Images/caesar.jpg'),
 ('/Images/cake.jpg'),('/Images/chicken.jpg'),('/Images/greek-salad.jpg'),('/Images/lobster-roll.jpg'),
 ('/Images/spinach-salad.jpg'),('/Images/tuscan-grilled.jpg'),('/Images/bas.jpg'),('/Images/crib.jpg')
) v(p) WHERE NOT EXISTS (SELECT 1 FROM GalleryItem g WHERE g.PictureUrl = v.p);

-- ---------- Kitchen team ----------
INSERT INTO ChefItem (ArName, EnName, Jop, ImageUrl)
SELECT v.ar, v.en, v.job, v.img FROM (VALUES
 (N'محمد السيد',  'Mohamed El-Sayed', 'Executive Chef',  '/Images/chefs-1.jpg'),
 (N'كريم حسن',    'Karim Hassan',     'Sous Chef',       '/Images/chefs-2.jpg'),
 (N'ليلى منصور',  'Layla Mansour',    'Pastry Chef',     '/Images/chefs-3.jpg'),
 (N'عمر فاروق',   'Omar Farouk',      'Grill Master',    '/Images/chif2.jpg'),
 (N'سارة عادل',   'Sara Adel',        'Salad & Cold Chef','/Images/chif3.jpg')
) v(ar,en,job,img) WHERE NOT EXISTS (SELECT 1 FROM ChefItem c WHERE c.EnName = v.en);

-- ---------- Menu sections (kitchen 1) ----------
INSERT INTO MenuSections (ArName, EnName, KitchenId)
SELECT v.ar, v.en, 1 FROM (VALUES
 (N'سلطات','Salads'),(N'برجر وساندويتش','Burgers & Sandwiches'),(N'دجاج','Chicken'),(N'مشروبات','Drinks')
) v(ar,en) WHERE NOT EXISTS (SELECT 1 FROM MenuSections s WHERE s.EnName = v.en);

-- ---------- Menu items (store 2; Size 4 = regular) ----------
DECLARE @salads INT=(SELECT Id FROM MenuSections WHERE EnName='Salads'),
        @burg   INT=(SELECT Id FROM MenuSections WHERE EnName='Burgers & Sandwiches'),
        @chick  INT=(SELECT Id FROM MenuSections WHERE EnName='Chicken'),
        @drinks INT=(SELECT Id FROM MenuSections WHERE EnName='Drinks'),
        @meat   INT=(SELECT Id FROM MenuSections WHERE EnName='Meat'),
        @fish   INT=(SELECT Id FROM MenuSections WHERE EnName='Fishes'),
        @sweet  INT=(SELECT Id FROM MenuSections WHERE EnName='Snikkers');

INSERT INTO MenuItems (ArName, EnName, MenuSectionId, Description, EnDescription, ImageUrl, StoreId, Price, IsReady, Size, IsAvailable)
SELECT v.ar, v.en, v.sec, v.ard, v.end_, v.img, 2, v.price, 1, 4, 1 FROM (VALUES
 (N'سلطة سيزر',        'Caesar Salad',        @salads, N'خس روماني وجبن بارميزان وكروتون مقرمش',        'Romaine, parmesan and crunchy croutons with Caesar dressing', '/Images/caesar.jpg',        85.00),
 (N'سلطة يونانية',     'Greek Salad',         @salads, N'خيار وطماطم وزيتون وجبن فيتا',                 'Cucumber, tomato, olives and feta with oregano',              '/Images/greek-salad.jpg',   80.00),
 (N'سلطة سبانخ',       'Spinach Salad',       @salads, N'سبانخ طازجة وفراولة وجوز مقرمش',               'Baby spinach, strawberries and candied walnuts',              '/Images/spinach-salad.jpg', 90.00),
 (N'برجر كلاسيك',      'Classic Burger',      @burg,   N'لحم بقري مشوي وجبن شيدر وصوص خاص',             'Grilled beef patty, cheddar and house sauce',                 '/Images/burger.jpg',       140.00),
 (N'ساندويتش لحم',     'Meat Sandwich',       @burg,   N'شرائح لحم طرية مع بصل مكرمل وجبن',             'Tender sliced beef with caramelised onion and cheese',        '/Images/meat%20sand.jpg',    120.00),
 (N'لوبستر رول',       'Lobster Roll',        @burg,   N'كركند طازج في خبز بريوش محمص بالزبدة',         'Fresh lobster in a buttered toasted brioche roll',            '/Images/lobster-roll.jpg', 260.00),
 (N'دجاج مشوي',        'Grilled Chicken',     @chick,  N'صدور دجاج متبلة مشوية مع خضار سوتيه',          'Marinated grilled chicken breast with sauteed vegetables',    '/Images/chicken.jpg',      150.00),
 (N'دجاج توسكاني مشوي','Tuscan Grilled Chicken',@chick,N'دجاج بالأعشاب الإيطالية وصلصة الليمون',        'Italian herb chicken with lemon sauce',                       '/Images/tuscan-grilled.jpg',165.00),
 (N'ستيك مشوي',        'Grilled Steak',       @meat,   N'قطعة ستيك مشوية على الفحم مع بطاطس',            'Charcoal-grilled steak served with potatoes',                 '/Images/food.jpg',         320.00),
 (N'سمك مشوي',         'Grilled Fish',        @fish,   N'سمك طازج مشوي مع أرز وسلطة',                   'Fresh grilled fish with rice and salad',                      '/Menu%20Images/fish.jpg',  210.00),
 (N'كيك الشوكولاتة',   'Chocolate Cake',      @sweet,  N'كيك شوكولاتة غني مع صوص دافئ',                 'Rich chocolate cake with warm sauce',                         '/Images/cake.jpg',          75.00),
 (N'حلويات شرقية',     'Oriental Sweets',     @sweet,  N'تشكيلة حلويات شرقية طازجة',                    'Assorted fresh oriental sweets',                              '/Menu%20Images/sweet.jpeg', 70.00),
 (N'قهوة',             'Coffee',              @drinks, N'قهوة محمصة طازجة',                              'Freshly roasted coffee',                                      '/Menu%20Images/coffee.jpg', 35.00),
 (N'شاي',              'Tea',                 @drinks, N'شاي ساخن بالنعناع',                             'Hot tea with fresh mint',                                     '/Menu%20Images/tea.webp',   20.00)
) v(ar,en,sec,ard,end_,img,price)
WHERE v.sec IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MenuItems m WHERE m.EnName = v.en);

COMMIT;

SELECT 'Gallery' AS T, COUNT(*) AS N FROM GalleryItem UNION ALL SELECT 'Chefs', COUNT(*) FROM ChefItem
UNION ALL SELECT 'Sections', COUNT(*) FROM MenuSections UNION ALL SELECT 'MenuItems', COUNT(*) FROM MenuItems;

