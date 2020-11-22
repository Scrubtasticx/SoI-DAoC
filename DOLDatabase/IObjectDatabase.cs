﻿/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections.Generic;

namespace DOL.Database
{
    /// <summary>
    /// Interface for Handling Object Database API
    /// </summary>
    public interface IObjectDatabase
    {
        /// <summary>
        /// Insert a new DataObject into the database and save it
        /// </summary>
        /// <param name="dataObject">DataObject to Add into database</param>
        /// <returns>True if the DataObject was added.</returns>
        bool AddObject(DataObject dataObject);

        /// <summary>
        /// Insert new DataObjects into the database and save them
        /// </summary>
        /// <param name="dataObjects">DataObjects to Add into database</param>
        /// <returns>True if All DataObjects were added.</returns>
        bool AddObject(IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Save a DataObject to database if saving is allowed and object is dirty
        /// </summary>
        /// <param name="dataObject">DataObject to Save in database</param>
        /// <returns>True if the DataObject was saved.</returns>
        bool SaveObject(DataObject dataObject);

        /// <summary>
        /// Save DataObjects to database if saving is allowed and object is dirty
        /// </summary>
        /// <param name="dataObjects">DataObjects to Save in database</param>
        /// <returns>True if All DataObjects were saved.</returns>
        bool SaveObject(IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Delete a DataObject from database if deletion is allowed
        /// </summary>
        /// <param name="dataObject">DataObject to Delete from database</param>
        /// <returns>True if the DataObject was deleted.</returns>
        bool DeleteObject(DataObject dataObject);

        /// <summary>
        /// Delete DataObjects from database if deletion is allowed
        /// </summary>
        /// <param name="dataObjects">DataObjects to Delete from database</param>
        /// <returns>True if All DataObjects were deleted.</returns>
        bool DeleteObject(IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Retrieve a DataObject from database based on its primary key value.
        /// </summary>
        /// <param name="key">Primary Key Value</param>
        /// <returns>Object found or null if not found</returns>
        TObject FindObjectByKey<TObject>(object key)
            where TObject : DataObject;
        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on their primary key values
        /// </summary>
        /// <param name="keys">Collection of Primary Key Values</param>
        /// <returns>Collection of DataObject with primary key matching values</returns>
        IList<TObject> FindObjectsByKey<TObject>(IEnumerable<object> keys)
            where TObject : DataObject;

        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on the Where Expression and Parameters Collection
        /// </summary>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="parameters">Collection of Parameters</param>
        /// <returns>Collection of Objects Sets for each matching Parametrized Query</returns>
        IList<IList<TObject>> SelectObjects<TObject>(string whereExpression, IEnumerable<IEnumerable<QueryParameter>> parameters)
            where TObject : DataObject;
        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on the Where Expression and Parameter Collection
        /// </summary>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="parameter">Collection of Parameter</param>
        /// <returns>Collection of Objects matching Parametrized Query</returns>
        IList<TObject> SelectObjects<TObject>(string whereExpression, IEnumerable<QueryParameter> parameter)
            where TObject : DataObject;
        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on the Where Expression and Parameter
        /// </summary>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="param">Single Parameter</param>
        /// <returns>Collection of Objects matching Parametrized Query</returns>
        IList<TObject> SelectObjects<TObject>(string whereExpression, QueryParameter param)
            where TObject : DataObject;

        /// <summary>
        /// Retrieve a Single DataObject from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <returns>Single Object or First Object if multiple matches</returns>
        [Obsolete("Use Parametrized Select Queries for best perfomances")]
        TObject SelectObject<TObject>(string whereExpression)
            where TObject : DataObject;
        /// <summary>
        /// Retrieve a Single DataObject from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Single Object or First Object if multiple matches</returns>
        [Obsolete("Use Parametrized Select Queries for best perfomances")]
        TObject SelectObject<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
            where TObject : DataObject;
        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <returns>Collection of DataObjects matching filter</returns>
        [Obsolete("Use Parametrized Select Queries for best perfomances")]
        IList<TObject> SelectObjects<TObject>(string whereExpression)
            where TObject : DataObject;
        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Collection of DataObjects matching filter</returns>
        [Obsolete("Use Parametrized Select Queries for best perfomances")]
        IList<TObject> SelectObjects<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
            where TObject : DataObject;

        /// <summary>
        /// Select all Objects From Table holding TObject Type
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Select</typeparam>
        /// <returns>Collection of all DataObject for this Type</returns>
        IList<TObject> SelectAllObjects<TObject>()
            where TObject : DataObject;
        /// <summary>
        /// Select all Objects From Table holding TObject Type
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Select</typeparam>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Collection of all DataObject for this Type</returns>
        IList<TObject> SelectAllObjects<TObject>(Transaction.IsolationLevel isolation)
            where TObject : DataObject;

        /// <summary>
        /// Gets the number of objects in a given table in the database.
        /// </summary>
        /// <typeparam name="TObject">the type of objects to retrieve</typeparam>
        /// <returns>a positive integer representing the number of objects; zero if no object exists</returns>
        int GetObjectCount<TObject>()
            where TObject : DataObject;

        /// <summary>
        /// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
        /// </summary>
        /// <typeparam name="TObject">the type of objects to retrieve</typeparam>
        /// <param name="whereExpression">the where clause to filter object count on</param>
        /// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
        int GetObjectCount<TObject>(string whereExpression)
            where TObject : DataObject;

        /// <summary>
        /// Register Data Object Type if not already Registered
        /// </summary>
        /// <param name="dataObjectType">DataObject Type</param>
        void RegisterDataObject(Type dataObjectType);

        /// <summary>
        /// Selects object from the database and updates or adds entry in the pre-cache.
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Query</typeparam>
        /// <param name="key">Key to Update</param>
        /// <returns>True if Object was found with given key</returns>
        bool UpdateInCache<TObject>(object key)
            where TObject : DataObject;

        /// <summary>
        /// Selects objects from the database and updates or adds entries in the pre-cache.
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Query</typeparam>
        /// <param name="keys">Key Collection to Update</param>
        /// <returns>True if All Objects were found with given keys</returns>
        bool UpdateObjsInCache<TObject>(IEnumerable<object> keys)
            where TObject : DataObject;

        /// <summary>
        /// Populate or Refresh Object Relations
        /// </summary>
        /// <param name="dataObject">Object to Populate</param>
        void FillObjectRelations(DataObject dataObject);

        /// <summary>
        /// Populate or Refresh Objects Relations
        /// </summary>
        /// <param name="dataObject">DataObject to Populate</param>
        void FillObjectRelations(IEnumerable<DataObject> dataObject);

        /// <summary>
        /// Escape wrong characters from string for Database Insertion
        /// </summary>
        /// <param name="rawInput">String to Escape</param>
        /// <returns>Escaped String</returns>
        string Escape(string rawInput);

        /// <summary>
        /// Execute a Raw Non-Query on the Database
        /// </summary>
        /// <param name="rawQuery">Raw Command</param>
        /// <returns>True if the Command succeeded</returns>
        [Obsolete("Raw Non-Query are SQL Only and don't use internal Object Database Implementations...")]
        bool ExecuteNonQuery(string rawQuery);
    }
}