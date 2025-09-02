class TransactionDao {
    constructor(cosmosClient, databaseId, collectionId) {
        this.client = cosmosClient;
        this.databaseId = databaseId;
        this.collectionId = collectionId;
        this.database = null;
        this.collection = null;

        this.init();
    }

    init = async () => {
        const dbResponse = await this.client.databases.createIfNotExists({id: this.databaseId});
        this.database = dbResponse.database;

        const coResponse = await this.database.containers.createIfNotExists({id: this.collectionId});
        this.collection = coResponse.container;
    }

    addItem = async (item, callback) => {
        item.date = Date.now();
        item.completed = false;

        try {
            console.log('addItem Data: ', item);
            await this.collection.items.create(item);
        } catch (err) {
            console.log('addItem Error Data:', err);
            callback (err);
        }
    }

    updateItem = async (item, callback) => {
        try {
            console.log('updateItem Data: ', item);
            await this.collection.items.upsert(item);
        }
        catch (err) {
            console.log('updateItem Error Data:', err);
            callback(err);
        }
    }

    getItem = async (itemId, callback) => {
        const querySpec = {
            query: 'SELECT * FROM c WHERE c.id = @id',
            parameters: [{
                name: '@id',
                value: itemId
            }]
        };

        console.log('getItem Data: ', querySpec);

        try {
            const { resources: results } = await this.collection.items.query(querySpec).fetchAll();
            if (results.length > 0) {
                console.log('getItem Result Data: ', results[0]);
                callback(null, results[0]);
            } else {
                callback(null);
            }
        }
        catch (err) {
            console.log('getItem Error Data:', err);
            callback(err);
        }
    }
}

module.exports = TransactionDao;
