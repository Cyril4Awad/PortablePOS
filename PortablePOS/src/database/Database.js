import { enablePromise, openDatabase, SQLiteDatabase, DEBUG } from 'react-native-sqlite-storage';

const dbName = 'portablePOS.db';
const foreignkeyEnforce = 'PRAGMA foreign_keys=1;'

enablePromise(true);
DEBUG(false);

export const getDBConnection = async () => {
  let DB = await openDatabase({ name: dbName, location: 'default', }).catch(err => { throw err });
  await executeNonQuery(foreignkeyEnforce, DB).catch(err => { throw err });
  return DB;
};

export const executeNonQuery = async (str, db = null) => {
  if (!db) {
    db = await getDBConnection().catch(err => { throw err });
  }
  let res = await db.executeSql(str).catch(err => { throw err });
  return res.length;
}

export const executeReader = async (str, db = null) => {
  if (!db) {
    db = await getDBConnection().catch(err => { throw err });
  }
  let res = await db.executeSql(str).catch(err => { throw err });
  if (res.length > 0) {
    let arr = [];
    for (let index = 0; index < res[0].rows.length; index++) {
      arr.push(res[0].rows.item(index))
    }
    return arr;
  }
  else {
    return null;
  }
}

export const executeWithinTransaction = async (str, db = null) => {
  let arr = [];
  if (Array.isArray(str)) {
    arr = str
  }
  else {
    arr.push(str);
  }

  if (!db) {
    db = await getDBConnection().catch(err => { throw err });
  }
  //awaitableTimeOut();
  let res = await executeinXactContextEntry(db, str).catch(err => { throw err });
  //console.log(res);
  return res
}

const executeInXactContext = (tx, str) => { 
  return new Promise((rsv, rjct) => {
    if (!str || str.length === 0) {
      rsv(true);
    }
    else {
      // console.log(str[0])
      tx.executeSql(str[0], [],
        (tx, rs) => {
          str.shift();
          rsv(executeInXactContext(tx, str));
        }, (err) => {
          console.log('rejecting ')
          rjct(err);
        });

    }
  })
}

const executeinXactContextEntry = (db, str) => {
  return new Promise((rs, rj) => {
    db.transaction((tx) => {
      executeInXactContext(tx, str).then(res => {
        //console.log('res', res)
        rs(res);
      }).catch(err => rj(err));//console.log('err',err));;

    })

  })
}

export const TransactionObject = (db) => new Promise((rs, rj) => {

  if (!db) {
    getDBConnection().then(db1 => {
      //  console.log(db1)
      db1.transaction(tx => {
        console.log(tx);
        rs(tx);
      }).catch(err => { throw err; });
    }).catch(err => {
      console.log(err)
      rj(err)
    });
  }


});













awaitableTimeOut = (tm) => new Promise((rs, rj) => {
  setTimeout(() => {
    rs(true)

  }, tm);

});





// export const getDBConnection = ()=> new Promise((rs,rj)=>{
//     console.log('here')
//     openDatabase({name : "testDB", createFromLocation : 2},
//     (db)=>{
//         console.log('resolving')
//         rs(db)
//     },
//     (err)=>
//     {
//         console.log(err);
//         rj(err)});
// });






// var db = SQLite.openDatabase({name : "testDB", createFromLocation : 2}, ()=>{console.log('ok')},(err)=>{console.log(err)});
// console.log(db);