### Tasks description for Lesson 4:

#### Комментарии:

Задание к уроку 4 выполнено.

Для отправки писем сделан класс Services.MailNotifier, реализующий простой интерфейс Services.IMailNotifier.

Класс MailNotifier я сделал Scoped, поскольку не уверен в потокобезопасности MailKit (и, даже если он потокобезопасен, потом может понадобится перейти на другую почтовую библиотеку, которая потокобезопасной не будет). Так что, мне кажется, ему правильней быть Scoped. Возможно, стоило бы сделать даже и Transient, чтобы потом иметь возможность отправить несколько писем за один запрос, запустив их отправку параллельно и асинхронно. Но пока я остановился на Scoped.

Но дальше я столкнулся с тем, что само хранилище данных должно быть Singleton - каталог должен быть единым для всех пользователей и всех их запросов-Scope'ов. А из Singleton каталога затруднительно использовать Scoped MailNofifier; использовать же MailNotifier из контроллера я не хотел, поскольку в задании было специально указано не переносить в контроллеры бизнес-логику.

Поэтому я разделил модель на два уровня - модель и хранилище. Нам в одном из прошлых курсов рассказывали, что так в любом случае правильнее делать - разделить Service и Repository. И поэтому сейчас есть:
- CatalogModel, реализующий интерфейс ICatalogModel, создается в режиме Scoped. И в основном пока просто является заглушкой для потенциальной бизнес-логики, просто транслируя все вызовы в хранилище. Но, помимо этого, CatalogModel использует MailNotifier (получая его через механизм DI) и отправляет оповещения на все операции с каталогом - добавление, удаление и изменение категорий и отдельных продуктов.
- CatalogStorage, реализующий интерфейс ICatalogStorage, создается в режиме Singleton - тем самым обеспечивая единое хранилище, с которым работают все запросы. При этом хранилище потокобезопасно за счет того, что основано на ConcurrentDictionary, это было сделано в прошлом уроке. Если в будущем хранилище надо будет перенести в базу данных, то тогда, возможно, его надо будет перенести в Scoped - в прошлых курсах показывали, что с DBContext надо работать так. Но пока хранилище реализовано на ConcurrentDictionary, режим Singleton единственно верный.

Кроме того, чтобы лучше соблюсти принципы DI, я решил максимально скрыть детали реализации хранилища. Теперь в CatalogData.cs описаны основные структуры данных - Product и Category, и скрыто то, что категория включает в себя коллекцию продуктов. А CatalogStorage уже объявляет приватного наследника от Category, в который включает ConcurrentDictionary<Product> и необходимые методы работы с ними. Пришлось немного повозиться, чтобы в нужных местах сделать преобразования типов, и вручную написать Enumerator'ы - но зато теперь интерфейсы (включающие в себя структуры данных в CatalogData и управляющие классы ICatalogStorage и ICatalogModel) абстрагированы от реализации. И, думаю, можно при необходимости, например, перевести хранилище в базу данных, сохранив неизменными интерфейсы. Кстати, для этого я сделал отдельные методы HasAny... для проверки пустоты хранилища - сейчас они возвращают просто Count>0. Но при использовании БД, полагаю, проверка непустоты - есть ли хоть одна запись - может быть существенно эффективней, чем подсчет всех записей, поэтому я в интерфейсе оставил на это задел.

Надеюсь, я правильно понял принципы DI, зачем они нужны и как ими пользоваться.